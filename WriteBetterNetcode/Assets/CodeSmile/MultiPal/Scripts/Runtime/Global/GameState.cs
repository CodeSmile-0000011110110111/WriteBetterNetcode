// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.MultiPal.Scene;
using CodeSmile.MultiPal.Settings;
using CodeSmile.Statemachine.Netcode;
using CodeSmile.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Global
{
	[DisallowMultipleComponent]
	public sealed class GameState : MonoBehaviour
	{
		[SerializeField] private GameStateBase[] m_GameStates = new GameStateBase[0];

		private Int32 m_ActiveStateIndex = -1;

		private GameStateBase ActiveState => m_ActiveStateIndex >= 0 ? m_GameStates[m_ActiveStateIndex] : null;

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

			EnterState(0);
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

		// FIXME: hardcoded
		private void WentOnline(NetcodeRole role) => EnterState(3);
		private void WentOffline(NetcodeRole role) => EnterState(2);

		private async void EnterState(Int32 stateIndex)
		{
			var clientSceneLoader = ComponentsRegistry.Get<ClientSceneLoader>();
			var serverSceneLoader = ComponentsRegistry.Get<ServerSceneLoader>();
			var currentState = ActiveState;
			var newGameState = m_GameStates[stateIndex];
			if (currentState == newGameState)
			{
				Debug.LogWarning($"tried to change GameState to already active state {newGameState} - ignoring");
				return;
			}

			Debug.Log($"<color=cyan> ================= GameState {newGameState.name} =================</color>");
			Debug.Log($"[{Time.frameCount}] GameState scene loading begins ...");

			var unloadScenes = GetScenesToUnload(currentState?.ClientSceneRefs, newGameState.ClientScenes);
			await clientSceneLoader.UnloadScenesAsync(unloadScenes);
			await clientSceneLoader.LoadScenesAsync(newGameState.ClientSceneRefs);

			unloadScenes = GetScenesToUnload(currentState?.ServerSceneRefs, newGameState.ServerScenes);
			await serverSceneLoader.UnloadScenesAsync(unloadScenes);
			await serverSceneLoader.LoadScenesAsync(newGameState.ServerSceneRefs);

			m_ActiveStateIndex = stateIndex;
			Debug.Log($"[{Time.frameCount}] GameState scene loading completed");
		}

		private SceneReference[] GetScenesToUnload(SceneReference[] loadedSceneRefs, AdditiveScene[] scenesToLoad)
		{
			// first state will have nothing to unload
			if (loadedSceneRefs == null || loadedSceneRefs.Length == 0)
				return new SceneReference[0];

			var unloadSceneRefs = new List<SceneReference>();
			for (var i = 0; i < loadedSceneRefs.Length; i++)
			{
				var alreadyLoadedScene = scenesToLoad.SingleOrDefault(scene => scene.Reference == loadedSceneRefs[i]);
				if (alreadyLoadedScene == null || alreadyLoadedScene.ForceReload)
					unloadSceneRefs.Add(loadedSceneRefs[i]);
			}

			return unloadSceneRefs.ToArray();
		}


		public void AdvanceState() =>
			// FIXME: placeholder
			EnterState(m_ActiveStateIndex + 1);
	}
}
