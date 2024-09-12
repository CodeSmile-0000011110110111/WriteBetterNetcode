// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.MultiPal.Scene
{
	[DisallowMultipleComponent]
	public sealed class ServerSceneLoader : MonoBehaviour
	{
		[Header("Debug")]
		[Tooltip("If checked, any scene loaded will be logged to the console.")]
		[SerializeField] private Boolean m_LogScenesBeingLoaded;
		private readonly HashSet<SceneReference> m_SynchedScenes = new();

		private Queue<SceneReference> m_ScenesToLoad;
		private String m_LoadingSceneName;
		private Boolean m_IsServerOnline;

		private void Awake() => ComponentsRegistry.Set(this);

		private void Start() => RegisterCallbacks();

		private void OnDestroy()
		{
			EndLoadAdditiveScenes();
			UnregisterCallbacks();
		}

		private void RegisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			netMan.OnServerStarted += OnServerStarted;
			netMan.OnServerStopped += OnServerStopped;
		}

		private void UnregisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.OnServerStarted -= OnServerStarted;
				netMan.OnServerStopped -= OnServerStopped;
			}
		}

		private void OnServerStarted()
		{
			m_IsServerOnline = true;
			var sceneManager = NetworkManager.Singleton.SceneManager;
			sceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);
			sceneManager.OnLoadEventCompleted += OnLoadCompletedForAll;
			sceneManager.OnSceneEvent += OnNetworkSceneEvent;
			//sceneManager.VerifySceneBeforeLoading += VerifySceneBeforeLoading;
		}

		private void OnServerStopped(Boolean isHost)
		{
			m_IsServerOnline = false;
			var sceneManager = NetworkManager.Singleton?.SceneManager;
			if (sceneManager != null)
			{
				sceneManager.OnLoadEventCompleted -= OnLoadCompletedForAll;
				sceneManager.OnSceneEvent -= OnNetworkSceneEvent;
				//sceneManager.VerifySceneBeforeLoading -= VerifySceneBeforeLoading;
			}

			StopAllCoroutines();
			EndLoadAdditiveScenes();
		}

		// private Boolean VerifySceneBeforeLoading(Int32 sceneIndex, String sceneName, LoadSceneMode loadMode)
		// {
		// 	Debug.Log($"VerifyScene {sceneIndex} {sceneName} {loadMode}");
		// 	return true;
		// }

		private void OnNetworkSceneEvent(SceneEvent sceneEvent) =>
			Debug.Log($"Server {sceneEvent.SceneEventType}: {sceneEvent.SceneName}");

		// private void BeginLoadAdditiveScenes()
		// {
		// 	// queue the additive scenes since we can only load one at a time
		// 	m_ScenesToLoad = new Queue<SceneReference>();
		// 	foreach (var sceneReference in m_AdditiveScenes)
		// 	{
		// 		if (sceneReference != null)
		// 			m_ScenesToLoad.Enqueue(sceneReference);
		// 	}
		//
		// 	// Start working on the queue
		// 	StartCoroutine(LoadAdditiveScenesRoutine());
		// }

		// private IEnumerator LoadAdditiveScenesRoutine()
		// {
		// 	var sceneMan = NetworkManager.Singleton.SceneManager;
		//
		// 	while (m_ScenesToLoad.Count > 0)
		// 	{
		// 		// single scene is probably still loading, wait for that to finish ...
		// 		if (m_LoadingSceneName != null)
		// 			yield return new WaitUntil(() => m_LoadingSceneName == null);
		//
		// 		m_LoadingSceneName = m_ScenesToLoad.Dequeue().SceneName;
		// 		if (m_LogScenesBeingLoaded)
		// 			NetworkLog.LogInfo($"{nameof(ServerSceneLoader)} adding: {m_LoadingSceneName}");
		//
		// 		sceneMan.LoadScene(m_LoadingSceneName, LoadSceneMode.Additive);
		//
		// 		// wait until load event marks the load as complete, which sets the field to null
		// 		yield return new WaitUntil(() => m_LoadingSceneName == null);
		// 	}
		//
		// 	EndLoadAdditiveScenes();
		// }

		private void EndLoadAdditiveScenes()
		{
			Debug.Log($"{nameof(ServerSceneLoader)} EndLoadAdditiveScenes");

			m_LoadingSceneName = null;
			m_ScenesToLoad = null;
		}

		public async Task UnloadAndLoadAdditiveScenesAsync(AdditiveScene[] additiveScenes)
		{
			if (m_IsServerOnline == false)
				return;

			await UnloadAllScenesAsync(additiveScenes);
			await LoadScenesAsync(additiveScenes);
		}

		public async Task UnloadAllScenesAsync(AdditiveScene[] scenesToKeep = null)
		{
			if (m_IsServerOnline == false)
				return;

			var scenesToUnload = new HashSet<SceneReference>(m_SynchedScenes);

			if (scenesToKeep != null)
			{
				foreach (var scene in scenesToKeep)
				{
					if (scene.ForceReload == false)
						scenesToUnload.Remove(scene.Reference);
				}
			}

			await LoadOrUnloadScenesAsync(scenesToUnload.ToArray(), false);
		}

		public async Task LoadScenesAsync(AdditiveScene[] scenes)
		{
			if (m_IsServerOnline == false)
				return;

			var sceneRefsToLoad = new SceneReference[scenes.Length];
			for (var i = 0; i < scenes.Length; i++)
				sceneRefsToLoad[i] = scenes[i].Reference;

			await LoadOrUnloadScenesAsync(sceneRefsToLoad, true);
		}

		private async Task LoadOrUnloadScenesAsync(SceneReference[] scenes, Boolean load)
		{
			if (scenes == null || scenes.Length == 0)
				return;

			var asyncOps = new List<AsyncOperation>();
			for (var i = 0; i < scenes.Length; i++)
			{
				Debug.Log($"LoadOrUnloadScenesAsync: {scenes[i].SceneName}");

				var asyncOp = load ? LoadSceneAsync(scenes[i]) : UnloadSceneAsync(scenes[i]);
				if (asyncOp != null)
					asyncOps.Add(asyncOp);
			}

			var tcs = new TaskCompletionSource<Boolean>();
			StartCoroutine(WaitForAsyncOperations(asyncOps, tcs));

			await tcs.Task;
		}

		private void OnLoadCompletedForAll(String sceneName, LoadSceneMode loadMode, List<UInt64> clientsCompleted,
			List<UInt64> clientsTimedOut)
		{
			if (sceneName == m_LoadingSceneName)
				m_LoadingSceneName = null;
		}

		private AsyncOperation LoadSceneAsync(SceneReference sceneRef)
		{
			if (m_SynchedScenes.Contains(sceneRef))
				return null;

			var sceneManager = NetworkManager.Singleton.SceneManager;
			if (sceneManager != null)
			{
				Debug.Log($"Server LoadScene: {sceneRef.SceneName}");
				m_SynchedScenes.Add(sceneRef);

				var status = sceneManager.LoadScene(sceneRef.SceneName, LoadSceneMode.Additive);

				// TODO: make awaitable
			}

			//throw new NotImplementedException();
			return null;
		}

		private AsyncOperation UnloadSceneAsync(SceneReference sceneRef)
		{
			if (m_SynchedScenes.Contains(sceneRef) == false)
				return null;

			Debug.Log($"Server UnloadScene: {sceneRef.SceneName}");
			m_SynchedScenes.Remove(sceneRef);
			return SceneManager.UnloadSceneAsync(sceneRef.SceneName);
		}

		private IEnumerator WaitForAsyncOperations(IEnumerable<AsyncOperation> asyncOps, TaskCompletionSource<Boolean> task)
		{
			foreach (var asyncOp in asyncOps)
				yield return asyncOp;

			task.SetResult(true);
		}
	}
}
