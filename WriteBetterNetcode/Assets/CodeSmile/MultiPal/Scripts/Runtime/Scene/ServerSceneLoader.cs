// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
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

		private SceneOperation m_SceneOperationMode;
		private Queue<SceneReference> m_ScenesProcessing;
		private TaskCompletionSource<Boolean> m_TaskInProgress;

		private Boolean m_IsServerOnline;
		private Boolean m_IsSceneEventCompleted;

		private NetworkManager NetworkManager => NetworkManager.Singleton;
		private NetworkSceneManager SceneManager => NetworkManager.Singleton?.SceneManager;

		private void Awake() => ComponentsRegistry.Set(this);

		private void Start() => RegisterCallbacks();

		private void OnDestroy()
		{
			EndLoadAdditiveScenes();
			UnregisterCallbacks();
		}

		private void RegisterCallbacks()
		{
			NetworkManager.OnServerStarted += OnServerStarted;
			NetworkManager.OnServerStopped += OnServerStopped;
		}

		private void UnregisterCallbacks()
		{
			if (NetworkManager != null)
			{
				NetworkManager.OnServerStarted -= OnServerStarted;
				NetworkManager.OnServerStopped -= OnServerStopped;
			}
		}

		private void OnServerStarted()
		{
			m_IsServerOnline = true;
			SceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);
			SceneManager.OnSceneEvent += OnNetworkSceneEvent;
			//SceneManager.VerifySceneBeforeLoading += VerifySceneBeforeLoading;
		}

		private void OnServerStopped(Boolean isHost)
		{
			m_IsServerOnline = false;
			if (SceneManager != null)
			{
				SceneManager.OnSceneEvent -= OnNetworkSceneEvent;
				//SceneManager.VerifySceneBeforeLoading -= VerifySceneBeforeLoading;
			}

			StopAllCoroutines();
			EndLoadAdditiveScenes();
		}

		private void EndLoadAdditiveScenes()
		{
			Debug.Log($"{nameof(ServerSceneLoader)} EndLoadAdditiveScenes");

			m_ScenesProcessing = null;

			if (m_TaskInProgress != null)
			{
				m_TaskInProgress.SetCanceled();
				m_TaskInProgress = null;
			}
		}

		private Boolean VerifySceneBeforeLoading(Int32 sceneIndex, String sceneName, LoadSceneMode loadMode)
		{
			Debug.Log($"VerifyScene {sceneIndex} {sceneName} {loadMode}");
			return true;
		}

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

		public async Task UnloadScenesAsync(SceneReference[] scenesToUnload) =>
			await InternalLoadUnloadScenesAsync(scenesToUnload, SceneOperation.Unload);

		public async Task LoadScenesAsync(SceneReference[] scenesToLoad) =>
			await InternalLoadUnloadScenesAsync(scenesToLoad, SceneOperation.Load);

		private async Task InternalLoadUnloadScenesAsync(SceneReference[] scenes, SceneOperation operation)
		{
			if (m_IsServerOnline == false)
				return;

			if (scenes == null || scenes.Length == 0)
				return;

			if (m_TaskInProgress != null || m_ScenesProcessing != null)
			{
				Debug.LogError("scene load/unload operation already in progress!");
				return;
			}

			m_SceneOperationMode = operation;
			m_ScenesProcessing = new Queue<SceneReference>(scenes);
			m_TaskInProgress = new TaskCompletionSource<Boolean>();

			ProcessNextSceneInQueue();

			await m_TaskInProgress.Task;
			m_TaskInProgress = null;
		}

		private void ProcessNextSceneInQueue()
		{
			Debug.Log(
				$"Scenes in queue: {m_ScenesProcessing.Count}, next: {(m_ScenesProcessing.Count > 0 ? m_ScenesProcessing.Peek() : null)}, mode: {m_SceneOperationMode}");

			if (m_ScenesProcessing.Count > 0)
			{
				m_IsSceneEventCompleted = false;

				var sceneRef = m_ScenesProcessing.Dequeue();
				if (m_SceneOperationMode == SceneOperation.Load)
				{
					Debug.Log($"Server will load: {sceneRef.SceneName}");
					InternalLoadQueuedSceneAsync(sceneRef);
				}
				else
				{
					Debug.Log($"Server will unload: {sceneRef.SceneName}");
					InternalUnloadQueuedSceneAsync(sceneRef);
				}
			}
			else
			{
				m_ScenesProcessing = null;
				m_TaskInProgress.SetResult(true);
			}
		}

		private void InternalLoadQueuedSceneAsync(SceneReference sceneRef)
		{
			if (m_SynchedScenes.Contains(sceneRef))
			{
				Debug.LogWarning($"skip load, already loaded: {sceneRef.SceneName}");
				ProcessNextSceneInQueue();
				return;
			}

			var status = SceneManager.LoadScene(sceneRef.SceneName, LoadSceneMode.Additive);

			if (status == SceneEventProgressStatus.Started)
				m_SynchedScenes.Add(sceneRef);
			else
			{
				Debug.LogWarning($"Scene load failed: {status} for {sceneRef.ScenePath}");
				ProcessNextSceneInQueue();
			}
		}

		private void InternalUnloadQueuedSceneAsync(SceneReference sceneRef)
		{
			if (m_SynchedScenes.Contains(sceneRef) == false)
			{
				Debug.LogWarning($"skip unload, not loaded: {sceneRef.SceneName}");
				ProcessNextSceneInQueue();
				return;
			}

			var status = SceneManager.UnloadScene(sceneRef.RuntimeScene);

			if (status == SceneEventProgressStatus.Started)
				m_SynchedScenes.Remove(sceneRef);
			else
			{
				Debug.LogWarning($"Scene unload failed: {status} for {sceneRef.ScenePath}");
				ProcessNextSceneInQueue();
			}
		}

		private IEnumerator ProcessNextSceneInQueueAfterCurrentEventCompleted()
		{
			yield return new WaitUntil(() => m_IsSceneEventCompleted);

			ProcessNextSceneInQueue();
		}

		private void OnNetworkSceneEvent(SceneEvent sceneEvent)
		{
			switch (sceneEvent.SceneEventType)
			{
				case SceneEventType.Load:
					StartCoroutine(ProcessNextSceneInQueueAfterCurrentEventCompleted());
					break;
				case SceneEventType.Unload:
					StartCoroutine(ProcessNextSceneInQueueAfterCurrentEventCompleted());
					break;
				case SceneEventType.Synchronize:
					break;
				case SceneEventType.ReSynchronize:
					break;
				case SceneEventType.LoadEventCompleted:
					break;
				case SceneEventType.UnloadEventCompleted:
					break;
				case SceneEventType.LoadComplete:
					m_IsSceneEventCompleted = true;
					break;
				case SceneEventType.UnloadComplete:
					m_IsSceneEventCompleted = true;
					break;
				case SceneEventType.SynchronizeComplete:
					break;
				case SceneEventType.ActiveSceneChanged:
					break;
				case SceneEventType.ObjectSceneChanged:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private enum SceneOperation
		{
			Load,
			Unload,
		}
	}
}
