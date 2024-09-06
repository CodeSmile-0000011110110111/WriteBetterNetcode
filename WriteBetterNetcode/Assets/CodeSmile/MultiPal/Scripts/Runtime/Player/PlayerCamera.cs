// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Player
{
	[DisallowMultipleComponent]
	public sealed class PlayerCamera : MonoBehaviour, IPlayerComponent
	{
		[SerializeField] private Transform m_CameraTarget;
		[SerializeField] private Transform m_LookAtTarget;
		private Int32 m_PlayerIndex = -1;
		public Transform Target => m_CameraTarget;
		public Transform LookAtTarget => m_LookAtTarget;

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;

			if (m_CameraTarget == null)
				m_CameraTarget = transform;

			var lookAtTarget = m_LookAtTarget != null ? m_LookAtTarget : m_CameraTarget;
			SetCinecamTargets(playerIndex, m_CameraTarget, lookAtTarget);
		}

		public void OnPlayerDespawn(Int32 playerIndex) {}

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

			var cameras = Global.Components.Cameras;
			foreach (var cinecam in cameras.GetPlayerCinecams(playerIndex))
				cinecam.Target = cameraTarget;
		}

		public void NextCamera()
		{
			var cameras = Global.Components.Cameras;
			cameras.SetNextCinecamEnabled(m_PlayerIndex);
		}
	}
}
