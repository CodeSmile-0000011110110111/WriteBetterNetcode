// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>(Owner only) Synchronizes a single rotation axis. </summary>
	/// <remarks>Compresses angle to byte, rotation increments are limited to 1.41 degrees.</remarks>
	public class NetworkRotationAxis : NetworkBehaviour
	{
		private const Single CompressionFactor = 360f / Byte.MaxValue; // = 1.4117..

		[Tooltip("The rotation axis to synchronize.")]
		[SerializeField] private AngleAxis m_SyncAxis;
		[Tooltip("Whether to synchronize local or world rotation.")]
		[SerializeField] private Boolean m_LocalSpace = true;
		[Tooltip("If true, interpolates rotation axis to the synchronized angle over Interpolation Time.")]
		[SerializeField] private Boolean m_Interpolate = true;
		[Tooltip("How long (in seconds) interpolation should last. Should not be more than a few frames (n * deltaTime).")]
		[SerializeField] private Single m_InterpolationTime = 0.06f;

		private readonly NetworkVariable<Byte> m_Angle = new(writePerm: NetworkVariableWritePermission.Owner);

		private Single m_InterpolationStartTime;
		private Single m_InterpolationStartAngle;
		private Boolean m_NeedsAngleUpdate;

		private void FixedUpdate()
		{
			if (IsOwner)
				m_Angle.Value = (Byte)Mathf.RoundToInt(GetSyncAngle() / CompressionFactor);
		}

		private void Update()
		{
			if (!IsOwner)
			{
				if (m_NeedsAngleUpdate)
				{
					m_NeedsAngleUpdate = false;

					var angle = m_Angle.Value * CompressionFactor;

					if (m_Interpolate)
					{
						var time = (Time.time - m_InterpolationStartTime) / m_InterpolationTime;
						angle = Mathf.LerpAngle(m_InterpolationStartAngle, angle, time);

						m_NeedsAngleUpdate = time < 1f;
					}

					var axisRotation = Quaternion.Euler(
						m_SyncAxis == AngleAxis.X ? angle : 0f,
						m_SyncAxis == AngleAxis.Y ? angle : 0f,
						m_SyncAxis == AngleAxis.Z ? angle : 0f);

					if (m_LocalSpace)
						transform.localRotation = axisRotation;
					else
						transform.rotation = axisRotation;
				}
			}
		}

		private Single GetSyncAngle()
		{
			var angles = m_LocalSpace ? transform.localRotation.eulerAngles : transform.rotation.eulerAngles;
			var angle = m_SyncAxis == AngleAxis.X ? angles.x : m_SyncAxis == AngleAxis.Y ? angles.y : angles.z;
			return angle < 0f ? angle + 360f : angle;
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (!IsOwner)
				m_Angle.OnValueChanged += OnAngleChanged;
		}

		public override void OnNetworkDespawn()
		{
			if (!IsOwner)
				m_Angle.OnValueChanged -= OnAngleChanged;

			base.OnNetworkDespawn();
		}

		private void OnAngleChanged(Byte previousAngle, Byte newAngle)
		{
			m_NeedsAngleUpdate = true;
			m_InterpolationStartTime = Time.time;
			m_InterpolationStartAngle = GetSyncAngle();
		}

		private enum AngleAxis
		{
			X,
			Y,
			Z,
		}
	}
}
