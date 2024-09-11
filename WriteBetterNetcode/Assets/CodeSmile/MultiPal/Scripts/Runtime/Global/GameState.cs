// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.MultiPal.Scene;
using CodeSmile.MultiPal.Settings;
using System;
using System.Collections;
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

			// all instantiated objects go in there, helps to see if there are any cleanup issues
			var activeScene = SceneManager.CreateScene("Runtime Instances");
			SceneManager.SetActiveScene(activeScene);
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

		private void WentOnline()
		{
			EnterState(m_GameStates[3]);
		}

		private void WentOffline()
		{
			EnterState(m_GameStates[2]);
		}

		private async void EnterState(GameStateBase gameState)
		{
			var clientSceneLoader = ComponentsRegistry.Get<ClientSceneLoader>();

			Debug.Log($"[{Time.frameCount}] start scene load");
			await clientSceneLoader.UnloadAndLoadAdditiveScenesAsync(gameState.ClientScenes);
			Debug.Log($"[{Time.frameCount}] load complete");

			var serverSceneLoader = ComponentsRegistry.Get<ServerSceneLoader>();
			await serverSceneLoader.UnloadAndLoadAdditiveScenesAsync(gameState.ServerScenes);

			//StartCoroutine(NextState());
		}

		private IEnumerator NextState()
		{
			yield return new WaitForSeconds(1f);

			if (m_ActiveStateIndex < 2)
			{
				m_ActiveStateIndex++;
				EnterState(m_GameStates[m_ActiveStateIndex]);
			}
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
