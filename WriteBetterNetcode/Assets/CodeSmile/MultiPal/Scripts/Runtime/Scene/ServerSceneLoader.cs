// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

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
		private readonly HashSet<SceneReference> m_LoadedScenes = new();

		private AsyncOperation LoadSceneAsync(SceneReference sceneRef)
		{
			if (m_LoadedScenes.Contains(sceneRef))
				return null;

			Debug.Log($"Server LoadScene: {sceneRef.SceneName}");
			m_LoadedScenes.Add(sceneRef);
			var sceneManager = NetworkManager.Singleton.SceneManager;
			var status = sceneManager.LoadScene(sceneRef.SceneName, LoadSceneMode.Additive);

			// TODO

			throw new NotImplementedException();
		}

		private AsyncOperation UnloadSceneAsync(SceneReference sceneRef)
		{
			if (m_LoadedScenes.Contains(sceneRef) == false)
				return null;

			Debug.Log($"Server UnloadScene: {sceneRef.SceneName}");
			m_LoadedScenes.Remove(sceneRef);
			return SceneManager.UnloadSceneAsync(sceneRef.SceneName);
		}

		public async Task UnloadAndLoadAdditiveScenesAsync(AdditiveScene[] additiveScenes)
		{
			await UnloadScenesWithExceptionsAsync(additiveScenes);
			await LoadScenesAsync(additiveScenes);
		}

		private async Task UnloadScenesWithExceptionsAsync(AdditiveScene[] scenesToKeep)
		{
			var scenesToUnload = new HashSet<SceneReference>(m_LoadedScenes);
			foreach (var keepScene in scenesToKeep)
			{
				if (keepScene.ForceReload == false)
					scenesToUnload.Remove(keepScene.Reference);
			}

			await UnloadScenesAsync(scenesToUnload.ToArray());
		}

		public async Task LoadScenesAsync(AdditiveScene[] scenesToLoad)
		{
			var sceneRefsToLoad = new SceneReference[scenesToLoad.Length];
			for (var i = 0; i < scenesToLoad.Length; i++)
				sceneRefsToLoad[i] = scenesToLoad[i].Reference;

			await LoadOrUnloadScenesAsync(sceneRefsToLoad, true);
		}

		public async Task LoadScenesAsync(SceneReference[] scenesToLoad) => await LoadOrUnloadScenesAsync(scenesToLoad, true);

		public async Task UnloadScenesAsync(SceneReference[] scenesToUnload) =>
			await LoadOrUnloadScenesAsync(scenesToUnload, false);

		private async Task LoadOrUnloadScenesAsync(SceneReference[] scenes, Boolean loadScenes)
		{
			var asyncOps = new List<AsyncOperation>();

			for (var i = 0; i < scenes.Length; i++)
			{
				var asyncOp = loadScenes
					? LoadSceneAsync(scenes[i])
					: UnloadSceneAsync(scenes[i]);

				if (asyncOp != null)
					asyncOps.Add(asyncOp);
			}

			var tcs = new TaskCompletionSource<Boolean>();
			StartCoroutine(WaitForAsyncOperations(asyncOps, tcs));

			await tcs.Task;
		}

		private IEnumerator WaitForAsyncOperations(IEnumerable<AsyncOperation> asyncOps, TaskCompletionSource<Boolean> task)
		{
			foreach (var asyncOp in asyncOps)
				yield return asyncOp;

			task.SetResult(true);
		}
	}
}
