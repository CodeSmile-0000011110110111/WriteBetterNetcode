// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

		private static void ThrowIfSceneReferenceNotValid(SceneReference sceneRef)
		{
			if (sceneRef == null || sceneRef.IsValid == false)
				throw new ArgumentException($"scene reference is null or invalid: {sceneRef}");
		}

		private void Awake() => ComponentsRegistry.Set(this);

		private AsyncOperation LoadSceneAsync(SceneReference sceneRef)
		{
			ThrowIfSceneReferenceNotValid(sceneRef);

			if (m_LoadedScenes.Contains(sceneRef))
				return null;

			Debug.Log($"Client LoadScene: {sceneRef.SceneName}");
			m_LoadedScenes.Add(sceneRef);
			return SceneManager.LoadSceneAsync(sceneRef.SceneName, LoadSceneMode.Additive);
		}

		private AsyncOperation UnloadSceneAsync(SceneReference sceneRef)
		{
			ThrowIfSceneReferenceNotValid(sceneRef);

			if (m_LoadedScenes.Contains(sceneRef) == false)
				return null;

			Debug.Log($"Client UnloadScene: {sceneRef.SceneName}");
			m_LoadedScenes.Remove(sceneRef);
			return SceneManager.UnloadSceneAsync(sceneRef.SceneName);
		}

		public async Task UnloadAndLoadAdditiveScenesAsync(AdditiveScene[] scenes)
		{
			await UnloadScenesWithExceptionsAsync(scenes);
			await LoadScenesAsync(scenes);
		}

		private async Task UnloadScenesWithExceptionsAsync(AdditiveScene[] scenesToKeep)
		{
			var scenesToUnload = new HashSet<SceneReference>(m_LoadedScenes);
			foreach (var scene in scenesToKeep)
			{
				if (scene.ForceReload == false)
					scenesToUnload.Remove(scene.Reference);
			}

			await UnloadScenesAsync(scenesToUnload.ToArray());
		}

		public async Task LoadScenesAsync(AdditiveScene[] scenes)
		{
			var sceneRefsToLoad = new SceneReference[scenes.Length];
			for (var i = 0; i < scenes.Length; i++)
				sceneRefsToLoad[i] = scenes[i].Reference;

			await LoadOrUnloadScenesAsync(sceneRefsToLoad, true);
		}

		public async Task LoadScenesAsync(SceneReference[] scenes) => await LoadOrUnloadScenesAsync(scenes, true);

		public async Task UnloadScenesAsync(SceneReference[] scenes) => await LoadOrUnloadScenesAsync(scenes, false);

		private async Task LoadOrUnloadScenesAsync(SceneReference[] scenes, Boolean load)
		{
			var asyncOps = new List<AsyncOperation>();

			for (var i = 0; i < scenes.Length; i++)
			{
				var asyncOp = load ? LoadSceneAsync(scenes[i]) : UnloadSceneAsync(scenes[i]);
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
