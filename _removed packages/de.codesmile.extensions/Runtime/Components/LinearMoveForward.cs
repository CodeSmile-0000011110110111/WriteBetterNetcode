// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.Components
{
	public class LinearMoveForward : MonoBehaviour
	{
		[SerializeField] private Single m_UnitsPerSecond = 1f;

		private Single m_UnitsPerTick;
		private Vector3 m_Velocity;

		public Single UnitsPerSecond => m_UnitsPerSecond;
		public Single UnitsPerTick => m_UnitsPerTick;
		public Vector3 Velocity => m_Velocity;

		private void Start()
		{
			m_UnitsPerTick = m_UnitsPerSecond * Time.fixedDeltaTime;
			m_Velocity = transform.forward * m_UnitsPerTick;
		}

		private void FixedUpdate() => transform.localPosition += m_Velocity;
	}
}
