// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Netcode;
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
	public sealed class ClientSceneLoader : MonoBehaviour
	{
		private readonly HashSet<SceneReference> m_LoadedScenesLocal = new();
		private readonly HashSet<SceneReference> m_LoadedScenesRemote = new();
		private TaskCompletionSource<Boolean> m_CompletionSource;
		private NetcodeState m_NetcodeState;

		private static void ThrowIfSceneReferenceNotValid(SceneReference sceneRef)
		{
			if (sceneRef == null || sceneRef.IsValid == false)
				throw new ArgumentException($"scene reference is null or invalid: {sceneRef}");
		}

		private void Awake()
		{
#if UNITY_EDITOR
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

			ComponentsRegistry.Set(this);
		}

		private async void Start()
		{
			m_NetcodeState = await ComponentsRegistry.GetAsync<NetcodeState>();
			m_NetcodeState.WentOnline += WentOnline;
			m_NetcodeState.WentOffline += WentOffline;
		}

		private void WentOnline()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan.IsClient)
				netMan.SceneManager.OnSceneEvent += OnNetworkSceneEvent;
		}

		private async void WentOffline()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null && netMan.SceneManager != null)
				netMan.SceneManager.OnSceneEvent -= OnNetworkSceneEvent;

			// unload all remote additive scenes when going offline
			Debug.LogWarning($"unload {m_LoadedScenesRemote.Count()} remote scenes");
			await UnloadScenesAsync(m_LoadedScenesRemote.ToArray());
		}

		private void OnNetworkSceneEvent(SceneEvent sceneEvent)
		{
			switch (sceneEvent.SceneEventType)
			{
				case SceneEventType.LoadComplete:
					var loadedSceneRef = new SceneReference(sceneEvent.Scene);
					m_LoadedScenesRemote.Add(loadedSceneRef);
					Debug.LogWarning($"client added remote scene: {loadedSceneRef.SceneName} -- {loadedSceneRef.ScenePath}");
					break;
				case SceneEventType.UnloadComplete:
					var unloadedSceneRef = new SceneReference(sceneEvent.Scene);
					m_LoadedScenesRemote.Remove(unloadedSceneRef);
					Debug.LogWarning(
						$"client removed remote scene: {unloadedSceneRef.SceneName} -- {unloadedSceneRef.ScenePath}");
					break;
				case SceneEventType.SynchronizeComplete:
					Debug.LogWarning($"synchronize event: {sceneEvent}");
					break;
			}
		}

		public async Task UnloadAndLoadAdditiveScenesAsync(AdditiveScene[] scenes)
		{
			await UnloadScenesAsync(scenes);
			await LoadScenesAsync(scenes);
		}

		private AsyncOperation LoadSceneAsync(SceneReference sceneRef)
		{
			ThrowIfSceneReferenceNotValid(sceneRef);

			if (m_LoadedScenesLocal.Contains(sceneRef))
				return null;

			Debug.Log($"Client LoadScene: {sceneRef.ScenePath}");
			m_LoadedScenesLocal.Add(sceneRef);
			return SceneManager.LoadSceneAsync(sceneRef.ScenePath, LoadSceneMode.Additive);
		}

		private AsyncOperation UnloadSceneAsync(SceneReference sceneRef)
		{
			ThrowIfSceneReferenceNotValid(sceneRef);

			if (m_LoadedScenesLocal.Contains(sceneRef) == false)
				return null;

			Debug.Log($"Client UnloadScene: {sceneRef.ScenePath}");
			m_LoadedScenesLocal.Remove(sceneRef);
			return SceneManager.UnloadSceneAsync(sceneRef.ScenePath);
		}

		private async Task UnloadScenesAsync(AdditiveScene[] scenesToKeep)
		{
			var scenesToUnload = new HashSet<SceneReference>(m_LoadedScenesLocal);
			foreach (var scene in scenesToKeep)
			{
				if (scene.ForceReload == false)
					scenesToUnload.Remove(scene.Reference);
			}

			await UnloadScenesAsync(scenesToUnload.ToArray());
		}

		private async Task UnloadScenesAsync(SceneReference[] scenes) => await LoadOrUnloadScenesAsync(scenes, false);

		private async Task LoadScenesAsync(AdditiveScene[] scenes)
		{
			var sceneRefsToLoad = new SceneReference[scenes.Length];
			for (var i = 0; i < scenes.Length; i++)
				sceneRefsToLoad[i] = scenes[i].Reference;

			await LoadOrUnloadScenesAsync(sceneRefsToLoad, true);
		}

		private async Task LoadScenesAsync(SceneReference[] scenes) => await LoadOrUnloadScenesAsync(scenes, true);

		private async Task LoadOrUnloadScenesAsync(SceneReference[] scenes, Boolean load)
		{
#if UNITY_EDITOR
			// avoid unload exception while exiting playmode
			if (m_IsExitingPlayMode)
				return;
#endif

			// nothing to do?
			if (scenes == null || scenes.Length == 0)
				return;

			var asyncOps = new List<AsyncOperation>();
			for (var i = 0; i < scenes.Length; i++)
			{
				var asyncOp = load ? LoadSceneAsync(scenes[i]) : UnloadSceneAsync(scenes[i]);
				if (asyncOp == null)
					throw new Exception($"async {(load ? "load" : "unload")} of '{scenes[i].SceneName}' returned null");

				asyncOps.Add(asyncOp);
			}

			if (m_CompletionSource != null)
				throw new InvalidOperationException("scene load/unload still in progress");

			// await completion of load/unload
			m_CompletionSource = new TaskCompletionSource<Boolean>();
			StartCoroutine(WaitForAsyncOperations(asyncOps, m_CompletionSource));
			await m_CompletionSource.Task;
			m_CompletionSource = null;
		}

		private IEnumerator WaitForAsyncOperations(IEnumerable<AsyncOperation> asyncOps, TaskCompletionSource<Boolean> task)
		{
			foreach (var asyncOp in asyncOps)
				yield return asyncOp;

			Debug.Log($"scene (un)load: all {asyncOps.Count()} async ops are done");
			task.SetResult(true);
		}

#if UNITY_EDITOR
		private Boolean m_IsExitingPlayMode;
		private void OnPlayModeStateChanged(PlayModeStateChange state) =>
			m_IsExitingPlayMode = state == PlayModeStateChange.ExitingPlayMode;
#endif
	}
}
