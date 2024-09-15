// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.PlayerController;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.MultiPal.Samples.RoboKyle.Controller
{
	public sealed class DeadPlayerController : PlayerControllerBase
	{
		[Header("Settings")]
		[SerializeField] private Single m_Gravity = -0.981f;
		[SerializeField] private Boolean m_InvertVertical;

		private Single m_DeltaTilt;
		private Single m_DeltaPan;

		private void Update()
		{
			// look before move, or else forward lags one update behind
			ApplyLook();
			ApplyMove();
		}

		private void ApplyMove()
		{
			m_Vertical.Value += m_Gravity * Time.deltaTime;
			m_Vertical.Validate();
		}

		public override void OnMove(InputAction.CallbackContext context)
		{
		}

		private void ApplyLook()
		{
			// tilting goes to camera tracking target as we don't want our viewmodel to tilt, just the camera
			m_Tilt.Value += m_DeltaTilt;
			m_Tilt.Validate();

			CameraTarget.localRotation = Quaternion.Euler(m_Tilt.Value, 0f, 0f);
		}

		public override void OnLook(InputAction.CallbackContext context)
		{
			var lookDir = context.ReadValue<Vector2>();
			m_DeltaTilt = lookDir.y * LookSensitivity.y * (m_InvertVertical ? 1f : -1f);
			m_DeltaPan = lookDir.x * LookSensitivity.x;
		}

		public override void OnCrouch(InputAction.CallbackContext context)
		{
		}

		public override void OnJump(InputAction.CallbackContext context)
		{
		}

		public override void OnSprint(InputAction.CallbackContext context) {}
	}
}
