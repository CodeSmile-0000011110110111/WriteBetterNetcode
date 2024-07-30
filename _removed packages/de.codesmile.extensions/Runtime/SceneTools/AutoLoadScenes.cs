// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace CodeSmile.SceneTools
{
	/// <summary>
	///     A list of scenes that are loaded automatically and additively every time the game launches (enter playmode, build).
	///     The scenes are guaranteed to be loaded before Awake() of any script in the first scene.
	/// </summary>
	[CreateAssetMenu(fileName = nameof(AutoLoadScenes), menuName = "Scriptable Objects/" + nameof(AutoLoadScenes))]
	public class AutoLoadScenes : ScriptableObject
	{
		[SerializeField] private List<SceneReference> m_AdditiveScenes;

		public IReadOnlyList<SceneReference> AdditiveScenes => m_AdditiveScenes.AsReadOnly();

		public static AutoLoadScenes Instance => Resources.Load<AutoLoadScenes>(nameof(AutoLoadScenes));

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void RuntimeInit_AdditiveLoadScenes()
		{
			var scenes = Instance?.AdditiveScenes;
			if (scenes == null)
				return;

			foreach (var sceneRef in scenes)
			{
				if (sceneRef != null && String.IsNullOrEmpty(sceneRef.SceneName) == false)
					SceneManager.LoadScene(sceneRef.SceneName, LoadSceneMode.Additive);
			}
		}

#if UNITY_EDITOR
		private static SceneReference GetSceneReference(Scene scene) =>
			new(AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path));
#endif

		private void OnValidate() => ValidateSceneReferences();

		private void ValidateSceneReferences()
		{
#if UNITY_EDITOR
			// make sure scene names are up to date
			foreach (var sceneRef in m_AdditiveScenes)
				sceneRef.OnValidate();

			// make sure we keep only unique names but retain the order
			var set = new List<SceneReference>();
			foreach (var sceneRef in m_AdditiveScenes)
			{
				if (sceneRef.SceneName != null && set.Contains(sceneRef) == false)
					set.Add(sceneRef);
			}

			m_AdditiveScenes = set.ToList();

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssetIfDirty(this);
#endif
		}

		public void AddScene(Scene scene)
		{
#if UNITY_EDITOR
			m_AdditiveScenes.Add(GetSceneReference(scene));
			ValidateSceneReferences();
#endif
		}

		public void RemoveScene(Scene scene)
		{
#if UNITY_EDITOR
			m_AdditiveScenes.Remove(GetSceneReference(scene));
			ValidateSceneReferences();
#endif
		}
	}
}
