// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.MultiPal
{
	public static class OpenMainScene
	{
		private static readonly String FirstTimeKey = "CodeSmileEditor.MultiPal." +
		                                              nameof(OpenMainScene) + PlayerSettings.productGUID +
		                                              ".FirstTimeImport_5";
		private static Boolean IsFirstTime
		{
			get => EditorPrefs.GetBool(FirstTimeKey, true);
			set => EditorPrefs.SetBool(FirstTimeKey, value);
		}

		[InitializeOnLoadMethod]
		private static void InitOnLoad()
		{
			if (IsFirstTime)
			{
				EditorApplication.delayCall += () =>
				{
					IsFirstTime = false;
					TryOpenMainScene();
				};
			}
		}

		private static void TryOpenMainScene()
		{
			const String MainScenePath = "Assets/CodeSmile/MultiPal/Scenes/### Global Content ###.unity";

			if (SceneManager.GetActiveScene().path.ToLower().Equals(MainScenePath.ToLower()))
				return;

			var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath);
			if (sceneAsset != null)
			{
				EditorSceneManager.SaveOpenScenes();
				EditorSceneManager.sceneOpened += OnSceneOpened;
				EditorSceneManager.OpenScene(MainScenePath);
			}
			else
			{
				// try again later, import may not have completed yet
				IsFirstTime = true;
			}
		}

		private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
		{
			EditorSceneManager.sceneOpened -= OnSceneOpened;
			EditorGUIUtility.PingObject(scene.GetRootGameObjects().FirstOrDefault());
		}
	}
}
