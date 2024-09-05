// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.MultiPal.Players.Controllers
{
	public sealed class SimplePlayerController : PlayerControllerBase
	{
		[Header("Settings")]
		[SerializeField] private Single m_Gravity = -1f;
		[SerializeField] private Boolean m_InvertVertical;

		private Single m_DeltaPan;
		private Single m_DeltaTilt;

		private void Update()
		{
			// look before move, or else forward lags one update behind
			ApplyLook();
			ApplyMove();
		}

		private void ApplyLook()
		{
			m_Tilt.Value += m_DeltaTilt;
			m_Pan.Value += m_DeltaPan;
			m_Tilt.Validate();
			m_Pan.Validate();

			// tilting goes to camera tracking target as we don't want our viewmodel to tilt, just the camera
			CameraTarget.localRotation = Quaternion.Euler(m_Tilt.Value, 0f, 0f);
			MotionTarget.localRotation = Quaternion.Euler(0f, m_Pan.Value, 0f);
		}

		private void ApplyMove()
		{
			m_Vertical.Value += m_Gravity * Time.deltaTime;
			m_Sideways.Validate();
			m_Forward.Validate();

			var right = MotionTarget.right;
			var forward = MotionTarget.forward;
			var moveDir = m_Sideways.Value * right + m_Forward.Value * forward;
			moveDir.y += m_Vertical.Value; // FIXME: this is likely incorrect, y will be a constant?

			CharController.Move(moveDir);
		}

		public override void OnMove(InputAction.CallbackContext context)
		{
			var moveDir = context.performed ? context.ReadValue<Vector2>() : Vector2.zero;
			m_Sideways.Value = moveDir.x * TranslationSensitivity.x * Time.deltaTime;
			m_Forward.Value = moveDir.y * TranslationSensitivity.z * Time.deltaTime;
		}

		public override void OnLook(InputAction.CallbackContext context)
		{
			//var lookDir = context.performed ? context.ReadValue<Vector2>() : Vector2.zero;
			var lookDir = context.ReadValue<Vector2>();
			m_DeltaTilt = lookDir.y * RotationSensitivity.y * Time.deltaTime * (m_InvertVertical ? 1f : -1f);
			m_DeltaPan = lookDir.x * RotationSensitivity.x * Time.deltaTime;
		}

		public override void OnCrouch(InputAction.CallbackContext context) {}

		public override void OnJump(InputAction.CallbackContext context)
		{
			if (context.performed)
				m_Vertical.Value = TranslationSensitivity.y;
		}

		public override void OnSprint(InputAction.CallbackContext context) {}
	}
}
