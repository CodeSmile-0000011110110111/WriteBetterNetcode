// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Players;
using CodeSmile.Settings;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	[DisallowMultipleComponent]
	public sealed class Cameras : MonoBehaviour
	{
		[Header("Cameras")]
		[Tooltip("Cameras used for everything besides Player's POV or Player tracking. " +
		         "Examples: Menu cam, Pre-/Post-Game view, Render cams.")]
		[SerializeField] private Camera[] m_OtherCameras;
		[Tooltip("The four player tracking cameras.")]
		[SerializeField] private Camera[] m_PlayerCameras = new Camera[Constants.MaxCouchPlayers];

		[Header("Cinemachine Cameras")]
		[Tooltip("The Cinemachine Camera prefabs the player can choose points of view from.")]
		[SerializeField] private PlayerCameraPrefabs m_PlayerCinecamPrefabs;
		[Tooltip("The Cinemachine Cameras that are enabled in 4-way splitscreen for player who haven't joined or left.")]
		[SerializeField] private CinemachineCamera[] m_PlayerNotJoinedCinecams;

		private readonly List<CinemachineCamera>[] m_PlayerCinecams = new List<CinemachineCamera>[Constants.MaxCouchPlayers];
		private Splitscreen m_Splitscreen;
		private Int32 m_OtherCameraIndex;

		public CinemachineCamera[] PlayerNotJoinedCinecams => m_PlayerNotJoinedCinecams;
		public Camera[] PlayerCameras => m_PlayerCameras;

		public Camera ActiveOtherCamera => m_OtherCameras[m_OtherCameraIndex];

		public Splitscreen Splitscreen => m_Splitscreen;

		private void Awake()
		{
			m_Splitscreen = GetComponent<Splitscreen>();

			if (m_PlayerCinecamPrefabs == null)
				throw new MissingReferenceException(nameof(PlayerCameraPrefabs));

			m_PlayerCinecamPrefabs.ValidatePrefabsHaveComponent<CinemachineCamera>();

			InitPlayerCinecams();
			ClearPlayerChannelsOfOtherCameras();
			SetPlayerCinecamBrainChannels();

			// always start with the offline camera
			SetDefaultCameraActive();
		}

		private void Start()
		{
			Components.OnLocalCouchPlayersSpawn += OnLocalCouchPlayersSpawn;
			Components.OnLocalCouchPlayersDespawn += OnLocalCouchPlayersDespawn;

			var netcodeState = Components.NetcodeState;
			netcodeState.WentOnline += WentOnline;
			netcodeState.WentOffline += WentOffline;
		}

		private void OnDestroy()
		{
			Components.OnLocalCouchPlayersSpawn -= OnLocalCouchPlayersSpawn;
			Components.OnLocalCouchPlayersDespawn -= OnLocalCouchPlayersDespawn;

			var netcodeState = Components.NetcodeState;
			if (netcodeState != null)
			{
				netcodeState.WentOnline -= WentOnline;
				netcodeState.WentOffline -= WentOffline;
			}
		}

		private void OnLocalCouchPlayersSpawn(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoined += OnCouchPlayerJoined;
			couchPlayers.OnCouchPlayerLeft += OnCouchPlayerLeft;
		}

		private void OnLocalCouchPlayersDespawn(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoined -= OnCouchPlayerJoined;
			couchPlayers.OnCouchPlayerLeft -= OnCouchPlayerLeft;
		}

		internal void OnCouchPlayerJoined(CouchPlayers couchPlayers, Int32 playerIndex)
		{
			SetCurrentOtherCameraActive(false);
			m_Splitscreen.UpdateSplitscreen(couchPlayers);
		}

		internal void OnCouchPlayerLeft(CouchPlayers couchPlayers, Int32 playerIndex)
		{
			Debug.Log($"player {playerIndex} left");
			m_Splitscreen.UpdateSplitscreen(couchPlayers);

			if (couchPlayers.PlayerCount == 0)
			{
				SetAllCamerasInactive(m_PlayerCameras);
				SetCurrentOtherCameraActive(true);
			}
		}

		public IReadOnlyList<CinemachineCamera> GetPlayerCinecams(Int32 playerIndex) =>
			m_PlayerCinecams[playerIndex].AsReadOnly();

		private void ClearPlayerChannelsOfOtherCameras()
		{
			var playerChannelsMask = OutputChannels.Channel01 | OutputChannels.Channel02 |
			                         OutputChannels.Channel03 | OutputChannels.Channel04;

			// other cameras should not get influenced by players' Cinecams
			for (var i = 0; i < m_OtherCameras.Length; i++)
			{
				var otherCamera = m_OtherCameras[i];
				if (otherCamera.TryGetComponent<CinemachineBrain>(out var brain))
					brain.ChannelMask &= ~playerChannelsMask; // clear player channels
			}
		}

		private void SetPlayerCinecamBrainChannels()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				var playerCamera = m_PlayerCameras[playerIndex];
				var brain = playerCamera.GetComponent<CinemachineBrain>();
				brain.ChannelMask = (OutputChannels)(1 << playerIndex + 1);
			}
		}

		private void InitPlayerCinecams()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				m_PlayerCameras[playerIndex].gameObject.SetActive(false);
				m_PlayerNotJoinedCinecams[playerIndex].gameObject.SetActive(false);

				m_PlayerCinecams[playerIndex] = new List<CinemachineCamera>();
				InstantiatePlayerCinecams(playerIndex, m_PlayerCinecamPrefabs);
			}
		}

		public void InstantiatePlayerCinecams(Int32 playerIndex, PlayerCameraPrefabs prefabs)
		{
			for (var cinecamIndex = 0; cinecamIndex < prefabs.Count; cinecamIndex++)
			{
				var cameraPrefab = prefabs[cinecamIndex];
				var cameraObj = Instantiate(cameraPrefab, transform);
				cameraObj.name = $"Player #{playerIndex}: [{cinecamIndex}] {cameraObj.name.Replace("(Clone)", "")}";

				var cinecam = cameraObj.GetComponent<CinemachineCamera>();
				cinecam.OutputChannel = (OutputChannels)(1 << playerIndex + 1);

				m_PlayerCinecams[playerIndex].Add(cinecam);
			}

			// make first one active - player's cinecams should all share same priority otherwise this won't work
			m_PlayerCinecams[playerIndex][0].Prioritize();
		}

		public void DestroyPlayerCinecams(Int32 playerIndex)
		{
			foreach (var playerCinecam in m_PlayerCinecams[playerIndex])
				Destroy(playerCinecam.gameObject);

			m_PlayerCinecams[playerIndex].Clear();
		}

		private void SetDefaultCameraActive() => SetCameraActive(0);

		private void SetCameraActive(Int32 cameraIndex)
		{
			Debug.Log($"SetCameraActive: {cameraIndex}");
			SetAllCamerasInactive(m_OtherCameras);
			SetAllCamerasInactive(m_PlayerCameras);
			m_OtherCameraIndex = cameraIndex;
			m_OtherCameras[cameraIndex].gameObject.SetActive(true);
		}

		private void SetAllCamerasInactive(Camera[] cameras)
		{
			foreach (var playerCamera in cameras)
			{
				var cameraObj = playerCamera.gameObject;
				if (cameraObj.activeSelf)
					cameraObj.SetActive(false);
			}
		}

		public void SetNextCinecamEnabled(Int32 playerIndex)
		{
			var cinecams = m_PlayerCinecams[playerIndex];
			for (var i = 0; i < cinecams.Count; i++)
			{
				var cinecam = cinecams[i];
				if (cinecam.IsLive)
				{
					var nextActiveCinecamIndex = i + 1;
					if (nextActiveCinecamIndex >= cinecams.Count)
						nextActiveCinecamIndex = 0;

					cinecams[nextActiveCinecamIndex].Prioritize();
					break;
				}
			}
		}

		private void WentOnline() => SetCameraActive(1);
		private void WentOffline() => SetDefaultCameraActive();

		private void SetCurrentOtherCameraActive(Boolean active) =>
			m_OtherCameras[m_OtherCameraIndex].gameObject.SetActive(active);
	}
}
