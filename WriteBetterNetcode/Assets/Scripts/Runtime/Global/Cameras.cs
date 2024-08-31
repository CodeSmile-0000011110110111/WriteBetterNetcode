// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

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
		[SerializeField] private Camera[] m_OtherCameras;
		[SerializeField] private Camera[] m_PlayerCameras = new Camera[Constants.MaxCouchPlayers];

		private readonly List<CinemachineCamera>[]
			m_PlayerCinecamLists = new List<CinemachineCamera>[Constants.MaxCouchPlayers];

		private void Awake()
		{
			AllocateCinecamLists();

			// always start with the offline camera
			SetDefaultCameraActive();
			SetPlayerCinecamBrainChannels();
		}

		private void Start()
		{
			var netcodeState = Components.NetcodeState;
			netcodeState.WentOnline += WentOnline;
			netcodeState.WentOffline += WentOffline;
		}

		private void OnDestroy()
		{
			var netcodeState = Components.NetcodeState;
			if (netcodeState != null)
			{
				netcodeState.WentOnline -= WentOnline;
				netcodeState.WentOffline -= WentOffline;
			}
		}

		public Camera GetPlayerCamera(Int32 playerIndex) => m_PlayerCameras[playerIndex];

		public IReadOnlyList<CinemachineCamera> GetPlayerCinecams(Int32 playerIndex) =>
			m_PlayerCinecamLists[playerIndex].AsReadOnly();

		private void AllocateCinecamLists()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
				m_PlayerCinecamLists[playerIndex] = new List<CinemachineCamera>();
		}

		private void SetPlayerCinecamBrainChannels()
		{
			for (var playerIndex = 0; playerIndex < m_PlayerCameras.Length; playerIndex++)
			{
				var playerCamera = m_PlayerCameras[playerIndex];
				var brain = playerCamera.GetComponent<CinemachineBrain>();
				brain.ChannelMask = (OutputChannels)(1 << playerIndex + 1);
			}
		}

		public void InstantiatePlayerCinecams(Int32 playerIndex, PlayerCameraPrefabs prefabs)
		{
			for (var cinecamIndex = 0; cinecamIndex < prefabs.Count; cinecamIndex++)
			{
				var cameraPrefab = prefabs[cinecamIndex];
				var cameraObj = Instantiate(cameraPrefab, transform);
				cameraObj.name = $"Player #{playerIndex}: {cameraObj.name.Replace("(Clone)", $"[{cinecamIndex}]")}";

				var cinecam = cameraObj.GetComponent<CinemachineCamera>();
				cinecam.OutputChannel = (OutputChannels)(1 << playerIndex + 1);

				m_PlayerCinecamLists[playerIndex].Add(cinecam);
			}

			// make first one active - player's cinecams should all share same priority otherwise this won't work
			m_PlayerCinecamLists[playerIndex][0].Prioritize();
		}

		public void DestroyPlayerCinecams(Int32 playerIndex)
		{
			foreach (var playerCinecam in m_PlayerCinecamLists[playerIndex])
				Destroy(playerCinecam.gameObject);

			m_PlayerCinecamLists[playerIndex].Clear();
		}

		private void SetDefaultCameraActive() => SetOtherCameraActive(0);

		private void SetOtherCameraActive(Int32 otherCameraIndex)
		{
			SetAllCamerasInactive(m_OtherCameras);
			SetAllCamerasInactive(m_PlayerCameras);
			m_OtherCameras[otherCameraIndex].gameObject.SetActive(true);
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

		public void SetPlayerCameraEnabled(Int32 playerIndex, Boolean enable)
		{
			SetAllCamerasInactive(m_OtherCameras);
			m_PlayerCameras[playerIndex].gameObject.SetActive(enable);
		}

		private void WentOnline() => SetOtherCameraActive(1);

		private void WentOffline() => SetDefaultCameraActive();
	}
}
