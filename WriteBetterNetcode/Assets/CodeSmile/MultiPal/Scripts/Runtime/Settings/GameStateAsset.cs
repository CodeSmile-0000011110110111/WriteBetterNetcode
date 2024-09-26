// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Scene;
using CodeSmile.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	[CreateAssetMenu(fileName = nameof(GameStateAsset), order = 1, menuName = "CodeSmile/" + nameof(GameStateAsset))]
	public class GameStateAsset : ScriptableObject
	{
		// TODO: may have multiple exit states
		[SerializeField] private GameStateConditionBase m_NextStateCondition;
		[SerializeField] private GameStateAsset m_NextState;

		[Tooltip("These scenes will be loaded or remain loaded and synchronized among clients when entering this state. " +
		         "Only the server (host) will load these scenes. IMPORTANT: Running network session required when " +
		         "entering this state!")]
		[SerializeField] private List<AdditiveScene> m_ServerScenes = new();

		[Tooltip("These scenes will be loaded or remain loaded on client-side when entering this state. " +
		         "The contents of these scenes are not synchronized with other clients or the server.")]
		[SerializeField] private List<AdditiveScene> m_ClientScenes = new();
		public GameStateConditionBase NextStateCondition => m_NextStateCondition;
		public GameStateAsset NextState => m_NextState;

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

		public virtual Boolean ConditionsSatisfied() => m_NextStateCondition != null && m_NextStateCondition.IsSatisfied();

		public virtual void OnEnterState(GameStateAsset fromState)
		{
			if (m_NextStateCondition != null)
				m_NextStateCondition.OnEnterState();
		}

		public virtual void OnExitState(GameStateAsset nextState)
		{
			if (m_NextStateCondition != null)
				m_NextStateCondition.OnExitState();
		}
	}
}
