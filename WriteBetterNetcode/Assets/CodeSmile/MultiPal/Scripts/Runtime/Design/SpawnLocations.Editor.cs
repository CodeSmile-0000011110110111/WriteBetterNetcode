// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.MultiPal.Design
{
	public sealed partial class SpawnLocations
	{
		internal void RegisterEditorSceneEvents()
		{
#if UNITY_EDITOR
			UnregisterEditorSceneEvents();
			EditorSceneManager.sceneSaving += OnsceneSaving;
			EditorSceneManager.sceneClosing += OnsceneClosing;
#endif
		}

		internal void UnregisterEditorSceneEvents()
		{
#if UNITY_EDITOR
			EditorSceneManager.sceneSaving -= OnsceneSaving;
			EditorSceneManager.sceneClosing -= OnsceneClosing;
#endif
		}

		private void UpdateSpawnLocationsList(Scene scene)
		{
#if UNITY_EDITOR
			// only update when saving our scene
			if (scene.path != gameObject.scene.path)
				return;

			var objectsFound = FindObjectsByType<SpawnLocation>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			Array.Sort(objectsFound, (l1, l2) => l1.name.CompareTo(l2.name));
			m_SpawnLocations = objectsFound;
#endif
		}

#if UNITY_EDITOR
		private void OnsceneClosing(Scene scene, Boolean removingscene)
		{
			if (scene.path == gameObject.scene.path)
				UnregisterEditorSceneEvents();
		}

		private void OnsceneSaving(Scene scene, String path) => UpdateSpawnLocationsList(scene);
#endif
	}
}
