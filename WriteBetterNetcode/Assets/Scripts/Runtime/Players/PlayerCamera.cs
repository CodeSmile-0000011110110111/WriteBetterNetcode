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
		[SerializeField] private Transform m_CameraTarget;
		[SerializeField] private Transform m_LookAtTarget;
		private Int32 m_PlayerIndex = -1;
		public Transform Target => m_CameraTarget;
		public Transform LookAtTarget => m_LookAtTarget;

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;

			var cameras = Components.Cameras;
			cameras.InstantiatePlayerCinecams(playerIndex, m_CameraPrefabs);
			cameras.SetPlayerCameraEnabled(playerIndex, true);

			if (m_CameraTarget == null)
				m_CameraTarget = transform;
			SetCinecamTargets(playerIndex, m_CameraTarget, m_LookAtTarget);
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

		public void SetTargets(Transform trackingTarget, Transform lookAtTarget = null) =>
			SetCinecamTargets(m_PlayerIndex, trackingTarget, lookAtTarget);

		private void SetCinecamTargets(Int32 playerIndex, Transform trackingTarget, Transform lookAtTarget)
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
