// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Scene;
using CodeSmile.MultiPal.Settings;
using System;
using System.Collections;
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
		}

		private void Start() => EnterState(m_GameStates[m_ActiveStateIndex]);

		private async void EnterState(GameStateBase gameState)
		{
			var sceneLoader = ComponentsRegistry.Get<ClientSceneLoader>();
			Debug.Log($"[{Time.frameCount}] start scene load");
			await sceneLoader.UnloadAndLoadAdditiveScenesAsync(gameState.ClientScenes);
			Debug.Log($"[{Time.frameCount}] load complete");

			StartCoroutine(NextState());
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
	}
}
