// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Animation;
using CodeSmile.MultiPal.Input;
using CodeSmile.MultiPal.Players;
using CodeSmile.MultiPal.Players.Couch;
using CodeSmile.MultiPal.Settings;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.PlayerController
{
	[DisallowMultipleComponent]
	public sealed class PlayerControllers : MonoBehaviour
	{
		[SerializeField] private PlayerControllerPrefabs m_ControllerPrefabs;
		[SerializeField] private Int32 m_DefaultControllerIndex; // FIXME: don't semi-hardcode this
		[SerializeField] private Int32 m_DeadPlayerControllerIndex; // FIXME: don't semi-hardcode this

		private readonly List<PlayerControllerBase>[] m_Controllers =
			new List<PlayerControllerBase>[Constants.MaxCouchPlayers];

		private readonly Int32[] m_ActiveControllerIndexes = new Int32[Constants.MaxCouchPlayers];

		private InputUsers m_InputUsers;

		private void Awake()
		{
			if (m_ControllerPrefabs == null)
				throw new MissingReferenceException(nameof(PlayerControllerPrefabs));

			ComponentsRegistry.Set(this);

			m_ControllerPrefabs.ValidatePrefabsHaveComponent<PlayerControllerBase>();

			AllocPlayerControllersLists();

			CouchPlayers.OnLocalCouchPlayersSpawn += OnLocalCouchPlayersSpawn;
			CouchPlayers.OnLocalCouchPlayersDespawn += OnLocalCouchPlayersDespawn;
		}

		private void OnDestroy()
		{
			CouchPlayers.OnLocalCouchPlayersSpawn -= OnLocalCouchPlayersSpawn;
			CouchPlayers.OnLocalCouchPlayersDespawn -= OnLocalCouchPlayersDespawn;
		}

		private void OnLocalCouchPlayersSpawn(CouchPlayers couchPlayers)
		{
			m_InputUsers = ComponentsRegistry.Get<InputUsers>();

			couchPlayers.OnCouchPlayerJoining += OnCouchPlayerJoining;
			couchPlayers.OnCouchPlayerJoined += OnCouchPlayerJoined;
			couchPlayers.OnCouchPlayerLeaving += OnCouchPlayerLeaving;
			couchPlayers.OnCouchPlayerLeft += OnCouchPlayerLeft;
		}

		private void OnLocalCouchPlayersDespawn(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoining -= OnCouchPlayerJoining;
			couchPlayers.OnCouchPlayerJoined -= OnCouchPlayerJoined;
			couchPlayers.OnCouchPlayerLeaving -= OnCouchPlayerLeaving;
			couchPlayers.OnCouchPlayerLeft -= OnCouchPlayerLeft;

			m_InputUsers = null;
		}

		private void OnCouchPlayerJoining(CouchPlayers couchPlayers, Int32 playerIndex) =>
			InstantiatePlayerControllers(playerIndex);

		private void OnCouchPlayerJoined(CouchPlayers couchPlayers, Int32 playerIndex)
		{
			var player = couchPlayers[playerIndex];
			var cameraTarget = player.Camera.TrackingTarget;
			if (cameraTarget == null)
				throw new ArgumentNullException($"player {playerIndex} camera tracking target");

			SetPlayerControllerTargets(playerIndex, player.transform, cameraTarget);
			SetControllerActive(playerIndex, m_DefaultControllerIndex);

			player.OnSwitchController += OnSwitchController;
			player.OnDeath += OnPlayerDeath;
			player.OnRespawn += OnPlayerRespawn;
		}

		private void OnCouchPlayerLeaving(CouchPlayers couchPlayers, Int32 playerIndex)
		{
			var player = couchPlayers[playerIndex];
			player.OnSwitchController -= OnSwitchController;
			player.OnDeath -= OnPlayerDeath;
			player.OnRespawn -= OnPlayerRespawn;

			m_InputUsers.SetPlayerKinematicsCallback(playerIndex, null);
			DestroyPlayerControllers(playerIndex);
		}

		private void OnCouchPlayerLeft(CouchPlayers couchPlayers, Int32 playerIndex) {}

		private void OnPlayerDeath(Player player) => SetControllerActive(player.PlayerIndex, m_DeadPlayerControllerIndex);
		private void OnPlayerRespawn(Player player) => SetControllerActive(player.PlayerIndex, m_DefaultControllerIndex);

		public PlayerControllerBase GetActiveController(Int32 playerIndex)
		{
			var activeIndex = m_ActiveControllerIndexes[playerIndex];
			return activeIndex >= 0 ? m_Controllers[playerIndex][activeIndex] : null;
		}

		private void AllocPlayerControllersLists()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				m_Controllers[playerIndex] = new List<PlayerControllerBase>();
				m_ActiveControllerIndexes[playerIndex] = -1;
			}
		}

		public void InstantiatePlayerControllers(Int32 playerIndex)
		{
			var prefabs = m_ControllerPrefabs;
			for (var ctrlIndex = 0; ctrlIndex < prefabs.Count; ctrlIndex++)
			{
				var ctrlPrefab = prefabs[ctrlIndex];
				var ctrlObj = Instantiate(ctrlPrefab, transform);
				ctrlObj.name = $"Player #{playerIndex}: {ctrlObj.name.Replace("(Clone)", $"[{ctrlIndex}]")}";
				ctrlObj.SetActive(false);

				var controller = ctrlObj.GetComponent<PlayerControllerBase>();
				controller.PlayerIndex = playerIndex;
				m_Controllers[playerIndex].Add(controller);
			}

			// make sure first controller is valid
			m_ActiveControllerIndexes[playerIndex] = m_DefaultControllerIndex;
		}

		public void SetPlayerControllerTargets(Int32 playerIndex, Transform motionTarget, Transform rotationTarget)
		{
			var playerControllers = m_Controllers[playerIndex];
			for (var ctrlIndex = 0; ctrlIndex < playerControllers.Count; ctrlIndex++)
			{
				var controller = playerControllers[ctrlIndex];
				controller.MotionTarget = motionTarget;
				controller.CameraTarget = rotationTarget;
			}
		}

		public void DestroyPlayerControllers(Int32 playerIndex)
		{
			foreach (var controller in m_Controllers[playerIndex])
				Destroy(controller.gameObject);

			m_Controllers[playerIndex].Clear();
			m_ActiveControllerIndexes[playerIndex] = -1;
		}

		public void SetControllerActive(Int32 playerIndex, Int32 controllerIndex)
		{
			// set current inactive
			GetActiveController(playerIndex)?.gameObject.SetActive(false);

			// activate new one
			m_ActiveControllerIndexes[playerIndex] = controllerIndex;

			var activeCtrl = GetActiveController(playerIndex);
			activeCtrl.gameObject.SetActive(true);
		}

		private void OnSwitchController(Int32 playerIndex) => SetNextControllerActive(playerIndex);

		public void SetPreviousControllerActive(Int32 playerIndex) => SetControllerActive(playerIndex,
			m_ActiveControllerIndexes[playerIndex] == 0
				? m_Controllers[playerIndex].Count - 1
				: m_ActiveControllerIndexes[playerIndex] - 1);

		public void SetNextControllerActive(Int32 playerIndex) => SetControllerActive(playerIndex,
			m_ActiveControllerIndexes[playerIndex] == m_Controllers[playerIndex].Count - 1
				? 0
				: m_ActiveControllerIndexes[playerIndex] + 1);

		public void SetAnimatorParameters(Int32 playerIndex, AvatarAnimatorParameters animParams)
		{
			foreach (var playerController in m_Controllers[playerIndex])
				playerController.AnimatorParameters = animParams;
		}
	}
}
