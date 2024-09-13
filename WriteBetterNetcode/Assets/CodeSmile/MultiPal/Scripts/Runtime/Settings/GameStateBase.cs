// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Scene;
using CodeSmile.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	public abstract class GameStateBase : ScriptableObject
	{
		[Tooltip("These scenes will be loaded or remain loaded and synchronized among clients when entering this state. " +
		         "Only the server (host) will load these scenes. IMPORTANT: Running network session required when " +
		         "entering this state!")]
		[SerializeField] private List<AdditiveScene> m_ServerScenes = new();

		[Tooltip("These scenes will be loaded or remain loaded on client-side when entering this state. " +
		         "The contents of these scenes are not synchronized with other clients or the server.")]
		[SerializeField] private List<AdditiveScene> m_ClientScenes = new();

		public AdditiveScene[] ClientScenes => m_ClientScenes.ToArray();
		public AdditiveScene[] ServerScenes => m_ServerScenes.ToArray();
		public SceneReference[] ClientSceneRefs => m_ClientScenes.Select(a => a.Reference).ToArray();
		public SceneReference[] ServerSceneRefs => m_ServerScenes.Select(a => a.Reference).ToArray();

		protected virtual void OnValidate()
		{
			foreach (var serverScene in m_ServerScenes)
				serverScene.Reference.OnValidate();
			foreach (var clientScene in m_ClientScenes)
				clientScene.Reference.OnValidate();
		}
	}
}
