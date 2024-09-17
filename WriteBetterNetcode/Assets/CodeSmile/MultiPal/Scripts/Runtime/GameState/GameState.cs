// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Scene;
using CodeSmile.MultiPal.Settings;
using CodeSmile.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.GameState
{
	[DisallowMultipleComponent]
	public sealed class GameState : MonoBehaviour
	{
		[SerializeField] private GameStateBase[] m_GameStates = new GameStateBase[0];

		private Int32 m_ActiveStateIndex = -1;

		private Boolean m_StateChangeInProgress;

		private GameStateBase ActiveState => m_ActiveStateIndex >= 0 ? m_GameStates[m_ActiveStateIndex] : null;

		private void Awake()
		{
			if (m_GameStates.Length == 0)
				throw new ArgumentException("no game states assigned!");

			ComponentsRegistry.Set(this);
		}

		private void Start() => SetActiveGameState(0);

		private void LateUpdate()
		{
			if (m_StateChangeInProgress == false)
				TryChangeState();
		}

		private void TryChangeState()
		{
			var activeState = ActiveState;
			if (activeState != null && activeState.ConditionsSatisfied())
			{
				var stateIndex = GetStateIndex(activeState.NextState);
				SetActiveGameState(stateIndex);
			}
		}

		private Int32 GetStateIndex(GameStateBase state) => Array.IndexOf(m_GameStates, state);

		private async void SetActiveGameState(Int32 stateIndex)
		{
			if (stateIndex < 0 || stateIndex >= m_GameStates.Length)
				throw new IndexOutOfRangeException($"stateIndex {stateIndex} out of range (has {m_GameStates.Length} states)");

			if (m_StateChangeInProgress)
				throw new InvalidOperationException("can't change GameState again - previous change still in progress!");

			var newGameState = m_GameStates[stateIndex];
			var currentState = ActiveState;
			if (currentState == newGameState)
			{
				Debug.LogWarning($"tried to change GameState to already active state {newGameState} - ignoring");
				return;
			}

			m_StateChangeInProgress = true;
			m_ActiveStateIndex = stateIndex;
			currentState?.OnExitState(newGameState);
			newGameState.OnEnterState(currentState);

			Debug.Log($"<color=cyan> ================= GameState {newGameState.name} =================</color>");
			Debug.Log($"[{Time.frameCount}] {newGameState.name} scene loading begins ...");

			{
				var unloadScenes = GetScenesToUnload(currentState?.ClientSceneRefs, newGameState.ClientScenes);
				var clientSceneLoader = ComponentsRegistry.Get<ClientSceneLoader>();
				await clientSceneLoader.UnloadScenesAsync(unloadScenes);
				await clientSceneLoader.LoadScenesAsync(newGameState.ClientSceneRefs);
			}
			{
				var unloadScenes = GetScenesToUnload(currentState?.ServerSceneRefs, newGameState.ServerScenes);
				var serverSceneLoader = ComponentsRegistry.Get<ServerSceneLoader>();
				await serverSceneLoader.UnloadScenesAsync(unloadScenes);
				await serverSceneLoader.LoadScenesAsync(newGameState.ServerSceneRefs);
			}

			m_StateChangeInProgress = false;
			Debug.Log($"[{Time.frameCount}] {newGameState.name} scene loading completed!");
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
	}
}
