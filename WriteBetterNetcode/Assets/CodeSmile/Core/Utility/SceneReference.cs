// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.Utility
{
	/// <summary>
	///     Allows drag & drop of scene assets in the Inspector and serializes the scene's name for runtime scene loading.
	/// </summary>
	[Serializable]
	public sealed class SceneReference : IEquatable<SceneReference>
	{
		[SerializeField] [HideInInspector] private String m_SceneName;

		/// <summary>
		///     The scene name of the assigned SceneAsset.
		/// </summary>
		public String SceneName => m_SceneName;
		public Boolean IsValid => String.IsNullOrWhiteSpace(m_SceneName) == false;
		public static Boolean operator ==(SceneReference left, SceneReference right) => Equals(left, right);
		public static Boolean operator !=(SceneReference left, SceneReference right) => !Equals(left, right);

		public SceneReference()
		{
#if UNITY_EDITOR
			// no need to unsubscribe these events
			PostProcessor.OnSceneAssetChanged += OnValidate;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
		}

		public Boolean Equals(SceneReference other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return m_SceneName == other.m_SceneName;
		}

		/// <summary>
		///     Call this from the MonoBehaviour that owns the SceneReference. Call it in OnValidate to ensure
		///     the SceneName is always in sync with the SceneAsset.
		/// </summary>
		public void OnValidate()
		{
#if UNITY_EDITOR
			SceneAsset = m_SceneAsset;

			if (m_SceneName != null)
			{
				var existsInBuildScenes = false;
				var scenePath = AssetDatabase.GetAssetOrScenePath(SceneAsset).ToLower();
				var buildScenes = EditorBuildSettings.scenes;
				foreach (var buildScene in buildScenes)
				{
					if (buildScene.enabled && buildScene.path.ToLower() == scenePath)
					{
						existsInBuildScenes = true;
						break;
					}
				}

				// TODO: auto-add to build index? should have an Inspector button for that
				if (existsInBuildScenes == false)
					Debug.LogWarning($"FYI: Scene '{m_SceneName}' is not in the build index yet or inactive.");
			}
#endif
		}

		public override String ToString() => $"{nameof(SceneReference)}({m_SceneName})";

		public override Boolean Equals(Object obj) =>
			ReferenceEquals(this, obj) || obj is SceneReference other && Equals(other);

		public override Int32 GetHashCode() => m_SceneName != null ? m_SceneName.GetHashCode() : 0;

#if UNITY_EDITOR
		[SerializeField] private SceneAsset m_SceneAsset;

		public SceneReference(SceneAsset sceneAsset) => SceneAsset = sceneAsset;

		public SceneAsset SceneAsset
		{
			get => m_SceneAsset;
			set
			{
				m_SceneAsset = value;
				m_SceneName = m_SceneAsset != null ? m_SceneAsset.name : null;
			}
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			// resubscribe when entering playmode since somehow we lose the connection
			PostProcessor.OnSceneAssetChanged -= OnValidate;
			PostProcessor.OnSceneAssetChanged += OnValidate;
		}

		internal class PostProcessor : AssetPostprocessor
		{
			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
			private static void ResetStaticFields() => OnSceneAssetChanged = null;

			private static void OnPostprocessAllAssets(String[] importedAssets, String[] deletedAssets, String[] movedAssets,
				String[] movedFromAssetPaths)
			{
				var deleteCount = deletedAssets.Length;
				var moveCount = movedAssets.Length;
				if (deleteCount > 0 || moveCount > 0)
					CheckSceneReferenceUpdates(deletedAssets.Concat(movedAssets));
			}

			private static void CheckSceneReferenceUpdates(IEnumerable<String> assets)
			{
				foreach (var asset in assets)
				{
					if (asset.EndsWith(".unity"))
					{
						// some scene changed, thus check all
						// we don't care about directing the change, it happens rarely
						OnSceneAssetChanged?.Invoke();
						break;
					}
				}
			}

			internal static event Action OnSceneAssetChanged;
		}

#endif
	}
}
