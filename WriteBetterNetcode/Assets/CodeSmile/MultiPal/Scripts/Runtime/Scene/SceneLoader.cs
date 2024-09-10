// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Utility;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.MultiPal.Scene
{
	[DisallowMultipleComponent]
	public sealed class SceneLoader : MonoBehaviour
	{
		public async Task LoadScenesAsync(SceneReference[] scenesToLoad) => await LoadOrUnloadScenesAsync(scenesToLoad, true);

		public async Task UnloadScenesAsync(SceneReference[] scenesToUnload) =>
			await LoadOrUnloadScenesAsync(scenesToUnload, false);

		private async Task LoadOrUnloadScenesAsync(SceneReference[] scenes, Boolean loadScenes)
		{
			var asyncOps = new AsyncOperation[scenes.Length];

			for (var i = 0; i < scenes.Length; i++)
				asyncOps[i] = loadScenes
					? SceneManager.LoadSceneAsync(scenes[i].SceneName, LoadSceneMode.Additive)
					: SceneManager.UnloadSceneAsync(scenes[i].SceneName);

			var tcs = new TaskCompletionSource<Boolean>();
			StartCoroutine(WaitForAsyncOperations(asyncOps, tcs));

			await tcs.Task;
		}

		private IEnumerator WaitForAsyncOperations(AsyncOperation[] asyncOps, TaskCompletionSource<Boolean> task)
		{
			foreach (var asyncOp in asyncOps)
				yield return asyncOp;

			task.SetResult(true);
		}
	}
}
