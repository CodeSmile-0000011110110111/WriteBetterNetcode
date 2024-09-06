// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Utility;
using CodeSmile.MultiPal.Global;
using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Player
{
	[DisallowMultipleComponent]
	public sealed class PlayerCamera : MonoBehaviour, IPlayerComponent
	{
		[SerializeField] private Transform m_CameraTrackingTarget;
		[SerializeField] private Transform m_LookAtTarget;
		private Int32 m_PlayerIndex = -1;
		public Transform TrackingTarget => m_CameraTrackingTarget;
		public Transform LookAtTarget => m_LookAtTarget;

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;

			if (m_CameraTrackingTarget == null)
				m_CameraTrackingTarget = transform;
		}

		public void OnPlayerDespawn(Int32 playerIndex) {}
	}
}
