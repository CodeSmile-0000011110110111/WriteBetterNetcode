// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.MultiPal.Scene;
using CodeSmile.MultiPal.Settings;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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

			Debug.Log($"[{Time.frameCount}] start client scene load: {gameState.name}");
			await clientSceneLoader.UnloadAndLoadAdditiveScenesAsync(gameState.ClientScenes);

			Debug.Log($"[{Time.frameCount}] start server scene load: {gameState.name}");
			var serverSceneLoader = ComponentsRegistry.Get<ServerSceneLoader>();
			await serverSceneLoader.UnloadAndLoadAdditiveScenesAsync(gameState.ServerScenes);

			Debug.Log($"[{Time.frameCount}] scene load completed: {gameState.name}");
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
