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
		[SerializeField] private Camera[] m_Cameras;
		[SerializeField] private Camera[] m_PlayerCameras = new Camera[Constants.MaxCouchPlayers];

		private readonly List<CinemachineCamera>[] m_PlayerCinecams = new List<CinemachineCamera>[Constants.MaxCouchPlayers];
		private Splitscreen m_Splitscreen;
		private Int32 m_ActiveCameraIndex;
		public Camera[] PlayerCameras => m_PlayerCameras;

		public Camera ActiveCamera => m_Cameras[m_ActiveCameraIndex];

		public Splitscreen Splitscreen => m_Splitscreen;

		private void Awake()
		{
			m_Splitscreen = GetComponent<Splitscreen>();

			AllocateCinecamCollection();

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

		public IReadOnlyList<CinemachineCamera> GetPlayerCinecams(Int32 playerIndex) =>
			m_PlayerCinecams[playerIndex].AsReadOnly();

		private void AllocateCinecamCollection()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
				m_PlayerCinecams[playerIndex] = new List<CinemachineCamera>();
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
			SetAllCamerasInactive(m_Cameras);
			SetAllCamerasInactive(m_PlayerCameras);
			m_ActiveCameraIndex = cameraIndex;
			m_Cameras[cameraIndex].gameObject.SetActive(true);
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
			SetAllCamerasInactive(m_Cameras);
			m_PlayerCameras[playerIndex].gameObject.SetActive(enable);
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

		public void SetCurrentCameraActive() => throw new NotImplementedException();
	}
}
