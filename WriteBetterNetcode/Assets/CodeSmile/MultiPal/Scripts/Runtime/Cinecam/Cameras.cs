// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Players.Couch;
using CodeSmile.MultiPal.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Cinecam
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Splitscreen))]
	public sealed class Cameras : MonoBehaviour
	{
		[Tooltip("Cameras used for everything besides Player's POV or Player tracking. " +
		         "Examples: Menu cam, Pre-/Post-Game view, Render cams.")]
		[SerializeField] private Camera[] m_OtherCameras;

		[Header("CouchPlayers' Modus Operandi")]
		[SerializeField] private CouchPlayerCameraMode m_CouchPlayerMode = CouchPlayerCameraMode.Splitscreen;

		[Header("Splitscreen Cameras")]
		[Tooltip("The Cinemachine Camera prefabs each player can switch between.")]
		[SerializeField] private PlayerCameraPrefabs m_PlayerCinecamPrefabs;
		[Tooltip("Where the Cinecams will be parented. If null, Cinecams will be children of this component.")]
		[SerializeField] private Transform m_PlayerCinecamRoot;
		[Tooltip("The four player-tracking cameras.")]
		[SerializeField] private Camera[] m_PlayerSplitCameras = new Camera[Constants.MaxCouchPlayers];
		[Tooltip("The Cinecams for players who haven't joined or left (4-way splitscreen only).")]
		[SerializeField] private CinemachineCamera[] m_PlayerNotJoinedCinecams;

		[Header("TargetGroup Camera")]
		[SerializeField] private Camera m_TargetGroupCamera;
		[SerializeField] private CinemachineCamera m_TargetGroupCinecam;
		[SerializeField] private CinemachineTargetGroup m_TargetGroup;

		private readonly List<CinemachineCamera>[] m_PlayerCinecams = new List<CinemachineCamera>[Constants.MaxCouchPlayers];
		private Splitscreen m_Splitscreen;
		private Int32 m_OtherCameraIndex;
		private IEnumerator m_UpdatePlayerCinecamNamesTimer;

		public CinemachineCamera[] PlayerNotJoinedCinecams => m_PlayerNotJoinedCinecams;
		public Camera[] PlayerSplitCameras => m_PlayerSplitCameras;

		public Camera ActiveOtherCamera => m_OtherCameras[m_OtherCameraIndex];

		public Splitscreen Splitscreen => m_Splitscreen;

		private void Awake()
		{
			ComponentsRegistry.Set(this);

			m_Splitscreen = GetComponent<Splitscreen>();

			if (m_PlayerCinecamPrefabs == null)
				throw new MissingReferenceException(nameof(PlayerCameraPrefabs));

			m_PlayerCinecamPrefabs.ValidatePrefabsHaveComponent<CinemachineCamera>();

			InitPlayerCinecams();
			ClearPlayerChannelsOfOtherCameras();
			SetPlayerCinecamBrainChannels();

			// always start with the offline camera
			SetDefaultCameraActive();

#if UNITY_EDITOR || DEBUG || DEVELOPMENT_BUILD
			StartCoroutine(DebugUpdatePlayerCinecamNamesTimer());
#endif
		}

		private void Start()
		{
			CouchPlayers.OnLocalCouchPlayersSpawn += OnLocalCouchPlayersSpawn;
			CouchPlayers.OnLocalCouchPlayersDespawn += OnLocalCouchPlayersDespawn;

			// check if couchplayers have already spawned
			var couchPlayers = ComponentsRegistry.Get<CouchPlayers>();
			if (couchPlayers != null)
			{
				OnLocalCouchPlayersSpawn(couchPlayers);

				for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
				{
					if (couchPlayers[playerIndex] != null)
						OnCouchPlayerJoined(couchPlayers, playerIndex);
				}
			}

			// FIXME: replace these with menu or gamestate events
			// var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			// netcodeState.WentOnline += WentOnline;
			// netcodeState.WentOffline += WentOffline;
		}

		private void OnDestroy()
		{
			CouchPlayers.OnLocalCouchPlayersSpawn -= OnLocalCouchPlayersSpawn;
			CouchPlayers.OnLocalCouchPlayersDespawn -= OnLocalCouchPlayersDespawn;

			// var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			// if (netcodeState != null)
			// {
			// 	netcodeState.WentOnline -= WentOnline;
			// 	netcodeState.WentOffline -= WentOffline;
			// }
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

		// private void WentOnline() => SetOtherCameraActive(1);
		// private void WentOffline() => SetDefaultCameraActive();

		internal void OnCouchPlayerJoined(CouchPlayers couchPlayers, Int32 playerIndex)
		{
			SetCurrentOtherCameraActive(false);

			SetCinecamTargets(couchPlayers, playerIndex);

			couchPlayers[playerIndex].OnSwitchCamera += SwitchToNextCinecam;

			if (m_CouchPlayerMode == CouchPlayerCameraMode.TargetGroup)
			{
				SetTargetGroupCameraActive(true);
				m_TargetGroup.Targets[playerIndex].Object = couchPlayers[playerIndex].transform;
			}
			else
				m_Splitscreen.UpdateSplitscreen(couchPlayers);
		}

		internal void OnCouchPlayerLeft(CouchPlayers couchPlayers, Int32 playerIndex)
		{
			couchPlayers[playerIndex].OnSwitchCamera -= SwitchToNextCinecam;

			if (m_CouchPlayerMode == CouchPlayerCameraMode.TargetGroup)
				m_TargetGroup.Targets[playerIndex].Object = null;
			else
				m_Splitscreen.UpdateSplitscreen(couchPlayers);

			if (couchPlayers.PlayerCount == 0)
			{
				SetTargetGroupCameraActive(false);
				SetAllCamerasInactive(m_PlayerSplitCameras);
				SetCurrentOtherCameraActive(true);
			}
		}

		private void SetCinecamTargets(CouchPlayers couchPlayers, Int32 playerIndex)
		{
			var playerCamera = couchPlayers[playerIndex].Camera;
			if (playerCamera.TrackingTarget == null)
				throw new ArgumentNullException($"player {playerIndex} camera tracking target");

			var cameraTarget = new CameraTarget
			{
				TrackingTarget = playerCamera.TrackingTarget,
				LookAtTarget = playerCamera.LookAtTarget,
				CustomLookAtTarget = playerCamera.LookAtTarget != null,
			};

			foreach (var cinecam in m_PlayerCinecams[playerIndex])
				cinecam.Target = cameraTarget;
		}

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
				var playerCamera = m_PlayerSplitCameras[playerIndex];
				var brain = playerCamera.GetComponent<CinemachineBrain>();
				var mask = (OutputChannels)(1 << playerIndex + 1);
				if (brain.ChannelMask != mask)
				{
					Debug.LogWarning($"{playerCamera.name} Cinebrain ChannelMask set to: {mask}");
					brain.ChannelMask = mask;
				}
			}
		}

		private void InitPlayerCinecams()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				m_PlayerSplitCameras[playerIndex].gameObject.SetActive(false);
				m_PlayerNotJoinedCinecams[playerIndex].gameObject.SetActive(false);

				m_PlayerCinecams[playerIndex] = new List<CinemachineCamera>();
				InstantiatePlayerCinecams(playerIndex, m_PlayerCinecamPrefabs);
			}
		}

		public void InstantiatePlayerCinecams(Int32 playerIndex, PlayerCameraPrefabs prefabs)
		{
			var parent = m_PlayerCinecamRoot != null ? m_PlayerCinecamRoot : transform;

			for (var cinecamIndex = 0; cinecamIndex < prefabs.Count; cinecamIndex++)
			{
				var cameraPrefab = prefabs[cinecamIndex];
				var cameraObj = Instantiate(cameraPrefab, parent);
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

		private void SetDefaultCameraActive() => SetOtherCameraActive(0);

		private void SetTargetGroupCameraActive(Boolean active)
		{
			m_TargetGroup.gameObject.SetActive(active);
			m_TargetGroupCamera.gameObject.SetActive(active);
			m_TargetGroupCinecam.gameObject.SetActive(active);
			if (m_TargetGroupCinecam.TryGetComponent<CinemachineGroupFraming>(out var framing))
				framing.enabled = active;
		}

		private void SetOtherCameraActive(Int32 cameraIndex)
		{
			Debug.Log($"SetOtherCameraActive: {cameraIndex}");
			SetAllCamerasInactive(m_OtherCameras);
			SetAllCamerasInactive(m_PlayerSplitCameras);
			SetTargetGroupCameraActive(false);

			m_OtherCameraIndex = cameraIndex;
			ActiveOtherCamera.gameObject.SetActive(true);
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

		public void SwitchToNextCinecam(Int32 playerIndex)
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

		private IEnumerator DebugUpdatePlayerCinecamNamesTimer()
		{
			do
			{
				yield return new WaitForSecondsRealtime(0.2f);

				DebugUpdatePlayerCinecamNames();
			} while (true);
		}

		private void DebugUpdatePlayerCinecamNames()
		{
			const String LiveSuffix = " (LIVE)";

			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				var cinecams = m_PlayerCinecams[playerIndex];
				for (var i = 0; i < cinecams.Count; i++)
				{
					var cinecam = cinecams[i];
					var cinecamGo = cinecam.gameObject;
					cinecamGo.name = cinecamGo.name.Replace(LiveSuffix, "");

					if (cinecam.IsLive)
						cinecamGo.name += " (LIVE)";
				}
			}
		}

		private void SetCurrentOtherCameraActive(Boolean active) =>
			m_OtherCameras[m_OtherCameraIndex].gameObject.SetActive(active);

		private enum CouchPlayerCameraMode
		{
			Splitscreen,
			TargetGroup,
		}
	}
}
