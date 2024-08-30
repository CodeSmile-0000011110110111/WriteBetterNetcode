// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Settings;
using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	public sealed class PlayerCamera : MonoBehaviour, IPlayerComponent
	{
		[SerializeField] private PlayerCameraPrefabs m_CameraPrefabs;

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			var cameras = Components.Cameras;
			cameras.InstantiatePlayerCinecams(playerIndex, m_CameraPrefabs, 0);
			cameras.SetPlayerCameraEnabled(playerIndex, true);
			SetCinecamTarget(playerIndex, transform);
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			var cameras = Components.Cameras;
			cameras.DestroyPlayerCinecams(playerIndex);
			cameras.SetPlayerCameraEnabled(playerIndex, false);
		}

		private void Awake()
		{
			if (m_CameraPrefabs == null)
				throw new MissingReferenceException(nameof(PlayerCameraPrefabs));

			m_CameraPrefabs.ValidatePrefabsHaveComponent<CinemachineCamera>();
		}

		private void SetCinecamTarget(Int32 playerIndex, Transform trackingTarget, Transform lookAtTarget = null)
		{
			var cameraTarget = new CameraTarget
			{
				TrackingTarget = trackingTarget,
				LookAtTarget = lookAtTarget,
				CustomLookAtTarget = lookAtTarget != null,
			};

			var cameras = Components.Cameras;
			foreach (var cinecam in cameras.GetPlayerCinecams(playerIndex))
				cinecam.Target = cameraTarget;
		}
	}
}
