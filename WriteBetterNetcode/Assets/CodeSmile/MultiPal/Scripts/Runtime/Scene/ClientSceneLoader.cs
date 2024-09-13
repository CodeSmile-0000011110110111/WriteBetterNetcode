// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.Statemachine.Netcode;
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

		private void WentOnline(NetcodeRole role)
		{
			var netMan = NetworkManager.Singleton;
			if (netMan.IsClient)
				netMan.SceneManager.OnSceneEvent += OnNetworkSceneEvent;
		}

		private async void WentOffline(NetcodeRole role)
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null && netMan.SceneManager != null)
				netMan.SceneManager.OnSceneEvent -= OnNetworkSceneEvent;

			// unload all remote additive scenes when going offline
			await InternalLoadOrUnloadScenesAsync(m_LoadedScenesRemote.ToArray(), false);
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

		public async Task UnloadScenesAsync(SceneReference[] scenes) => await InternalLoadOrUnloadScenesAsync(scenes, false);
		public async Task LoadScenesAsync(SceneReference[] scenes) => await InternalLoadOrUnloadScenesAsync(scenes, true);

		public AsyncOperation UnloadSceneAsync(SceneReference sceneRef)
		{
			if (sceneRef == null)
				return null;

			if (m_LoadedScenesLocal.Contains(sceneRef) == false)
				return null;

			Debug.Log($"<color=orange>Client UnloadScene: {sceneRef.SceneName}</color>");
			m_LoadedScenesLocal.Remove(sceneRef);
			return SceneManager.UnloadSceneAsync(sceneRef.ScenePath);
		}

		public AsyncOperation LoadSceneAsync(SceneReference sceneRef)
		{
			if (sceneRef == null)
				return null;

			if (m_LoadedScenesLocal.Contains(sceneRef))
				return null;

			Debug.Log($"<color=green>Client LoadScene: {sceneRef.SceneName}</color>");
			m_LoadedScenesLocal.Add(sceneRef);
			return SceneManager.LoadSceneAsync(sceneRef.ScenePath, LoadSceneMode.Additive);
		}

		private async Task InternalLoadOrUnloadScenesAsync(SceneReference[] scenes, Boolean load)
		{
#if UNITY_EDITOR
			// avoid exception while exiting playmode since some component may trigger a scene unload
			if (m_IsExitingPlayMode)
				return;
#endif

			// nothing to do?
			if (scenes == null || scenes.Length == 0)
				return;

			var asyncOps = new List<AsyncOperation>();
			for (var i = 0; i < scenes.Length; i++)
			{
				var scene = scenes[i];
				if (scene == null)
					continue;

				var asyncOp = load ? LoadSceneAsync(scene) : UnloadSceneAsync(scene);
				if (asyncOp == null)
					throw new Exception($"async {(load ? "load" : "unload")} of '{scene.SceneName}' returned null");

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

			task.SetResult(true);
		}

		public Boolean IsSceneLoaded(String sceneName)
		{
			if (m_LoadedScenesLocal != null)
			{
				foreach (var sceneReference in m_LoadedScenesLocal)
				{
					if (sceneReference.SceneName.Equals(sceneName) || sceneReference.ScenePath.Equals(sceneName))
						return true;
				}
			}
			return false;
		}

#if UNITY_EDITOR
		private Boolean m_IsExitingPlayMode;
		private void OnPlayModeStateChanged(PlayModeStateChange state) =>
			m_IsExitingPlayMode = state == PlayModeStateChange.ExitingPlayMode;
#endif
	}
}
