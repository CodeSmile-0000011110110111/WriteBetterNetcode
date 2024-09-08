// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.MultiPal.Utility
{
	public sealed class SceneManagerNoSingleLoad : SceneManagerAPI
	{
		private const String SingleSceneLoadNotSupportedMessage =
			"LoadSceneMode.Single is not supported in MultiPal! Scenes have to be loaded additively and unloaded as needed. " +
			"Design your content into objects/components that exist all the time, and the content that only exists for a " +
			"small period of time, eg a menu or indoor environment. It does not make a conceptual difference, it requires " +
			"a change of 'habit' in organizing content and an additional 'unload' of content when it is no longer needed.";

		[RuntimeInitializeOnLoadMethod]
		private static void OnRuntimeMethodLoad() => overrideAPI = new SceneManagerNoSingleLoad();

		protected override AsyncOperation LoadSceneAsyncByNameOrIndex(String sceneName, Int32 sceneBuildIndex,
			LoadSceneParameters parameters, Boolean mustCompleteNextFrame)
		{
			if (parameters.loadSceneMode == LoadSceneMode.Single)
				throw new NotSupportedException(SingleSceneLoadNotSupportedMessage);

			return base.LoadSceneAsyncByNameOrIndex(sceneName, sceneBuildIndex, parameters, mustCompleteNextFrame);
		}
	}
}
