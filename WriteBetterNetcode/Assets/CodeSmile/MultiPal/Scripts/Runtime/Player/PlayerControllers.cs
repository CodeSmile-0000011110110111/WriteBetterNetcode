// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Settings;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Player
{
	[DisallowMultipleComponent]
	public sealed class PlayerControllers : MonoBehaviour
	{
		[SerializeField] private PlayerControllerPrefabs m_ControllerPrefabs;

		private readonly List<PlayerControllerBase>[] m_Controllers =
			new List<PlayerControllerBase>[Constants.MaxCouchPlayers];

		private readonly Int32[] m_ActiveControllers = new Int32[Constants.MaxCouchPlayers];

		private void Awake()
		{
			if (m_ControllerPrefabs == null)
				throw new MissingReferenceException(nameof(PlayerControllerPrefabs));

			m_ControllerPrefabs.ValidatePrefabsHaveComponent<PlayerControllerBase>();

			AllocPlayerControllersLists();
		}

		private void Start()
		{
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
			couchPlayers.OnCouchPlayerJoined += OnCouchPlayerJoined;
			couchPlayers.OnCouchPlayerLeaving += OnCouchPlayerLeaving;
			couchPlayers.OnCouchPlayerLeft += OnCouchPlayerLeft;
		}

		private void OnLocalCouchPlayersDespawn(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoined -= OnCouchPlayerJoined;
			couchPlayers.OnCouchPlayerLeaving -= OnCouchPlayerLeaving;
			couchPlayers.OnCouchPlayerLeft -= OnCouchPlayerLeft;
		}

		private void OnCouchPlayerJoined(CouchPlayers couchPlayers, Int32 playerIndex)
		{
			var player = couchPlayers[playerIndex];
			var cameraTarget = player.Camera.TrackingTarget;
			InstantiatePlayerControllers(playerIndex, player.transform, cameraTarget);
		}

		private void OnCouchPlayerLeaving(CouchPlayers couchPlayers, Int32 playerIndex) =>
			DestroyPlayerControllers(playerIndex);

		private void OnCouchPlayerLeft(CouchPlayers couchPlayers, Int32 playerIndex) {}

		public PlayerControllerBase GetActiveController(Int32 playerIndex)
		{
			var activeIndex = m_ActiveControllers[playerIndex];
			return activeIndex >= 0 ? m_Controllers[playerIndex][activeIndex] : null;
		}

		private void AllocPlayerControllersLists()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				m_Controllers[playerIndex] = new List<PlayerControllerBase>();
				m_ActiveControllers[playerIndex] = -1;
			}
		}

		public void InstantiatePlayerControllers(Int32 playerIndex, Transform motionTarget, Transform rotationTarget)
		{
			var prefabs = m_ControllerPrefabs;
			for (var ctrlIndex = 0; ctrlIndex < prefabs.Count; ctrlIndex++)
			{
				var ctrlPrefab = prefabs[ctrlIndex];
				var ctrlObj = Instantiate(ctrlPrefab, transform);
				ctrlObj.name = $"Player #{playerIndex}: {ctrlObj.name.Replace("(Clone)", $"[{ctrlIndex}]")}";
				ctrlObj.SetActive(false);

				var controller = ctrlObj.GetComponent<PlayerControllerBase>();
				controller.MotionTarget = motionTarget;
				controller.CameraTarget = rotationTarget;
				m_Controllers[playerIndex].Add(controller);

				if (ctrlIndex == 0)
					SetControllerActive(playerIndex, ctrlIndex);
			}
		}

		public void DestroyPlayerControllers(Int32 playerIndex)
		{
			foreach (var controller in m_Controllers[playerIndex])
			{
				Debug.Log($"destroying ctrl {controller.gameObject.name}");
				Destroy(controller.gameObject);
			}

			m_Controllers[playerIndex].Clear();
			m_ActiveControllers[playerIndex] = -1;
		}

		public void SetControllerActive(Int32 playerIndex, Int32 controllerIndex)
		{
			// check if already active
			if (m_ActiveControllers[playerIndex] == controllerIndex)
				return;

			// deactivate current, activate new one
			GetActiveController(playerIndex)?.gameObject.SetActive(false);
			m_ActiveControllers[playerIndex] = controllerIndex;
			GetActiveController(playerIndex)?.gameObject.SetActive(true);
		}

		public void SetPreviousControllerActive(Int32 playerIndex) => SetControllerActive(playerIndex,
			m_ActiveControllers[playerIndex] == 0
				? m_Controllers[playerIndex].Count - 1
				: m_ActiveControllers[playerIndex] - 1);

		public void SetNextControllerActive(Int32 playerIndex) => SetControllerActive(playerIndex,
			m_ActiveControllers[playerIndex] == m_Controllers[playerIndex].Count - 1
				? 0
				: m_ActiveControllers[playerIndex] + 1);
	}
}
