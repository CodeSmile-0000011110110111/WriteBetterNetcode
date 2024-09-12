// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.MultiPal.Scene;
using CodeSmile.MultiPal.Settings;
using CodeSmile.Utility;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Global
{
	[DisallowMultipleComponent]
	public sealed class GameState : MonoBehaviour
	{
		[SerializeField] private GameStateBase[] m_GameStates = new GameStateBase[0];

		private Int32 m_ActiveStateIndex;

		private void Awake()
		{
			if (m_GameStates.Length == 0)
				throw new ArgumentException("no game states assigned!");

			ComponentsRegistry.Set(this);
		}

		private void Start()
		{
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			netcodeState.WentOnline += WentOnline;
			netcodeState.WentOffline += WentOffline;

			EnterState(m_GameStates[m_ActiveStateIndex]);
		}

		private void OnDestroy()
		{
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			if (netcodeState != null)
			{
				netcodeState.WentOnline -= WentOnline;
				netcodeState.WentOffline -= WentOffline;
			}
		}

		private void WentOnline() => EnterState(m_GameStates[3]);

		private void WentOffline() => EnterState(m_GameStates[2]);

		private async void EnterState(GameStateBase gameState)
		{
			var clientSceneLoader = ComponentsRegistry.Get<ClientSceneLoader>();

			Debug.Log($"<color=cyan> ================= GameState {gameState.name} =================</color>");
			await clientSceneLoader.UnloadAndLoadAdditiveScenesAsync(gameState.ClientScenes);

			var serverSceneRefs = new SceneReference[gameState.ServerScenes.Length];
			for (var i = 0; i < gameState.ServerScenes.Length; i++)
				serverSceneRefs[i] = gameState.ServerScenes[i].Reference;

			var serverSceneLoader = ComponentsRegistry.Get<ServerSceneLoader>();
			await serverSceneLoader.UnloadScenesAsync(serverSceneRefs);
			await serverSceneLoader.LoadScenesAsync(serverSceneRefs);

			Debug.Log($"[{Time.frameCount}] GameState scene loading completed");
		}

		private void ExitState(GameStateBase gameState) {}

		public void AdvanceState()
		{
			// FIXME: placeholder
			m_ActiveStateIndex++;
			EnterState(m_GameStates[m_ActiveStateIndex]);
		}
	}
}
