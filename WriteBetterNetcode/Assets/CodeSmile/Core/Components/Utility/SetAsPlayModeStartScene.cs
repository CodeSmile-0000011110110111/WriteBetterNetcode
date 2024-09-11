﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace CodeSmile.Components
{
	/// <summary>
	///     Sets the scene with this script in it as the PlayMode start scene. Allows to enter PlayMode with a scene other
	///     than the one currently being edited to avoid frequent scene switching for testing.
	/// </summary>
	/// <remarks>
	///     Disable either the GameObject or the component to restore default PlayMode behaviour, eg start with the
	///     currently open scene.
	/// </remarks>
	[ExecuteAlways] [DisallowMultipleComponent]
	public sealed class SetAsPlayModeStartScene : MonoBehaviour
	{
#if UNITY_EDITOR
		private const String PlayModeScenePathKey = "CodeSmile.EnterPlayModeStartScenePath";

		private Boolean m_IsPlayModeStartSceneClosing;

		private void OnEnable()
		{
			RegisterCallbacks();
			SetActiveSceneAsPlayModeStartScene();
		}

		private void OnDisable() => SetActiveSceneAsPlayModeStartScene();

		private void SetActiveSceneAsPlayModeStartScene()
		{
			if (m_IsPlayModeStartSceneClosing || EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			if (enabled && gameObject.activeInHierarchy)
			{
				var scenePath = SceneManager.GetActiveScene().path;
				var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
				if (EditorSceneManager.playModeStartScene != sceneAsset)
				{
					SessionState.SetString(PlayModeScenePathKey, scenePath);
					EditorSceneManager.playModeStartScene = sceneAsset;
					Debug.Log($"PlayMode StartScene is now: {sceneAsset.name}");
				}
			}
			else if (EditorSceneManager.playModeStartScene != null)
			{
				SessionState.SetString(PlayModeScenePathKey, null);
				EditorSceneManager.playModeStartScene = null;
				Debug.Log("PlayMode StartScene is now: (active scene)");
			}
		}

		private void RegisterCallbacks()
		{
			EditorSceneManager.sceneClosing -= OnSceneClosing;
			EditorSceneManager.sceneClosing += OnSceneClosing;
			EditorSceneManager.sceneClosed -= OnSceneClosed;
			EditorSceneManager.sceneClosed += OnSceneClosed;

			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
		}

		private void UnregisterCallbacks()
		{
			EditorSceneManager.sceneClosing -= OnSceneClosing;
			EditorSceneManager.sceneClosed -= OnSceneClosed;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
		}

		private void OnSceneClosing(Scene scene, Boolean removingscene) => m_IsPlayModeStartSceneClosing =
			m_IsPlayModeStartSceneClosing || EditorSceneManager.playModeStartScene?.name == scene.name;

		private void OnSceneClosed(Scene scene)
		{
			if (m_IsPlayModeStartSceneClosing)
				UnregisterCallbacks();
		}

		private void OnAfterAssemblyReload() => RestorePlayModeStartScene();

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredEditMode)
			{
				RegisterCallbacks();
				RestorePlayModeStartScene();
			}
		}

		private static void RestorePlayModeStartScene()
		{
			var scenePath = SessionState.GetString(PlayModeScenePathKey, null);
			var startScene = scenePath != null ? AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) : null;
			EditorSceneManager.playModeStartScene = startScene;
		}

#else
		private void Awake() => Destroy(this);
#endif
	}
}
