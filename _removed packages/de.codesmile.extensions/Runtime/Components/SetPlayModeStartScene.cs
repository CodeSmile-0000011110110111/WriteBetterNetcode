// Copyright (C) 2021-2024 Steffen Itterheim
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
	///     Sets the currently active scene as the PlayMode start scene. Allows to enter PlayMode with a given scene even
	///     while editing another scene.
	/// </summary>
	/// <remarks>
	///     Disable either the GameObject or the component to restore default PlayMode behaviour (run the currently open
	///     scene).
	/// </remarks>
	[ExecuteAlways] [DisallowMultipleComponent]
	public class SetPlayModeStartScene : MonoBehaviour
	{
#if UNITY_EDITOR
		private void Awake() => RegisterCallbacks();
		private void Reset() => SetActiveSceneAsPlayModeStartScene();
		private void OnValidate() => SetActiveSceneAsPlayModeStartScene();
		private void OnEnable() => SetActiveSceneAsPlayModeStartScene();
		private void OnDisable() => SetActiveSceneAsPlayModeStartScene();
		private void OnDestroy() => UnregisterCallbacks();

		private void SetActiveSceneAsPlayModeStartScene()
		{
			if (m_IsSceneClosing || EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			if (enabled && gameObject.activeInHierarchy)
			{
				var scenePath = SceneManager.GetActiveScene().path;
				var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
				if (EditorSceneManager.playModeStartScene != sceneAsset)
				{
					SessionState.SetString(PlayModeScenePathKey, scenePath);
					EditorSceneManager.playModeStartScene = sceneAsset;
					Debug.Log($"PlayMode StartScene: {sceneAsset.name}");
				}
			}
			else if (EditorSceneManager.playModeStartScene != null)
			{
				SessionState.SetString(PlayModeScenePathKey, null);
				EditorSceneManager.playModeStartScene = null;
				Debug.Log("PlayMode StartScene: (active scene)");
			}
		}

		private void OnSceneClosing(Scene scene, Boolean removingscene) =>
			m_IsSceneClosing = SceneManager.GetActiveScene() == scene;

		private void RegisterCallbacks()
		{
			EditorSceneManager.sceneClosing += OnSceneClosing;

			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
		}

		private void UnregisterCallbacks() => EditorSceneManager.sceneClosing -= OnSceneClosing;
		private static void OnAfterAssemblyReload() => RestorePlayModeStartScene();

		private static void OnPlayModeStateChanged(PlayModeStateChange mode)
		{
			if (mode == PlayModeStateChange.EnteredEditMode)
				RestorePlayModeStartScene();
		}

		private static void RestorePlayModeStartScene()
		{
			var scenePath = SessionState.GetString(PlayModeScenePathKey, null);
			EditorSceneManager.playModeStartScene =
				scenePath != null ? AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) : null;
		}

		private const String PlayModeScenePathKey = "CodeSmile.EnterPlayModeStartScenePath";
		private Boolean m_IsSceneClosing;
#else
		private void Awake() => Destroy(this);
#endif
	}
}
