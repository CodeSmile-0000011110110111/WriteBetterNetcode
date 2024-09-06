// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players
{
	[DisallowMultipleComponent]
	public sealed class PlayerCamera : MonoBehaviour, IPlayerComponent
	{
		[SerializeField] private Transform m_TrackingTarget;
		[SerializeField] private Transform m_LookAtTarget;
		public Transform TrackingTarget => m_TrackingTarget;
		public Transform LookAtTarget => m_LookAtTarget;

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			if (m_TrackingTarget == null)
				m_TrackingTarget = transform;
		}

		public void OnPlayerDespawn(Int32 playerIndex) {}
	}
}
