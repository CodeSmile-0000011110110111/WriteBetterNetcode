// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.MultiPal.Scene
{
	[DisallowMultipleComponent]
	public sealed class ClientSceneLoader : MonoBehaviour
	{
		private readonly HashSet<SceneReference> m_LoadedScenes = new();
		private Int32 m_AsyncOperationsCount;
		private TaskCompletionSource<Boolean> m_CompletionSource;

		private void Awake()
		{
#if UNITY_EDITOR
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

			ComponentsRegistry.Set(this);
		}

		public async Task UnloadScenesAsync(SceneReference[] scenes) => await InternalLoadOrUnloadScenesAsync(scenes, false);
		public async Task LoadScenesAsync(SceneReference[] scenes) => await InternalLoadOrUnloadScenesAsync(scenes, true);

		public AsyncOperation UnloadSceneAsync(SceneReference sceneRef)
		{
			if (sceneRef == null)
				return null;

			if (m_LoadedScenes.Contains(sceneRef) == false)
				return null;

			Debug.Log($"<color=orange>Client UnloadScene: {sceneRef.SceneName}</color>");
			m_LoadedScenes.Remove(sceneRef);
			return SceneManager.UnloadSceneAsync(sceneRef.ScenePath);
		}

		public AsyncOperation LoadSceneAsync(SceneReference sceneRef)
		{
			if (sceneRef == null)
				return null;

			if (m_LoadedScenes.Contains(sceneRef))
				return null;

			Debug.Log($"<color=green>Client LoadScene: {sceneRef.SceneName}</color>");
			m_LoadedScenes.Add(sceneRef);
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

			if (m_CompletionSource != null)
				throw new InvalidOperationException("scene load/unload still in progress - await the completion!");

			m_CompletionSource = new();
			m_AsyncOperationsCount = scenes.Length;

			for (var i = 0; i < scenes.Length; i++)
			{
				var scene = scenes[i];
				if (scene == null)
					throw new ArgumentNullException("a scene is null");

				var asyncOp = load ? LoadSceneAsync(scene) : UnloadSceneAsync(scene);
				if (asyncOp == null)
					throw new($"async {(load ? "load" : "unload")} of '{scene.SceneName}' returned null");

				asyncOp.completed += OnSceneOperationComplete;
			}

			// await completion of load/unload
			await m_CompletionSource.Task;
			m_CompletionSource = null;
			m_AsyncOperationsCount = 0;
		}

		private void OnSceneOperationComplete(AsyncOperation asyncOp)
		{
			asyncOp.completed -= OnSceneOperationComplete;

			m_AsyncOperationsCount--;
			if (m_AsyncOperationsCount == 0)
				m_CompletionSource.SetResult(true);
		}

		public Boolean IsSceneLoaded(String sceneName)
		{
			if (m_LoadedScenes != null)
			{
				foreach (var sceneReference in m_LoadedScenes)
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
