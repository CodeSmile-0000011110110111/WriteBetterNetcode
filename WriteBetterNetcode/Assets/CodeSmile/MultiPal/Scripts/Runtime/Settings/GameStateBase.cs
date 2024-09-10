// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Scene;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	public abstract class GameStateBase : ScriptableObject
	{
		[Tooltip("These scenes will be loaded or remain loaded and synchronized among clients when entering this state. " +
		         "Only the server (host) will load these scenes. IMPORTANT: Running network session required when " +
		         "entering this state!")]
		[SerializeField] private AdditiveScene[] m_ServerScenes = new AdditiveScene[0];

		[Tooltip("These scenes will be loaded or remain loaded on client-side when entering this state. " +
		         "The contents of these scenes are not synchronized with other clients or the server.")]
		[SerializeField] private AdditiveScene[] m_ClientScenes = new AdditiveScene[0];

		public AdditiveScene[] ClientScenes => m_ClientScenes;

		protected virtual void OnValidate()
		{
			foreach (var serverScene in m_ServerScenes)
				serverScene.Reference.OnValidate();
			foreach (var clientScene in m_ClientScenes)
				clientScene.Reference.OnValidate();
		}
	}
}
