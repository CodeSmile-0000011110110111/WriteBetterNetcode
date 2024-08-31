// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Players.Controllers
{
	public sealed class SimplePlayerController : PlayerControllerBase
	{
		[Header("Settings")]
		[SerializeField] private Single m_Gravity = -1f;
		[SerializeField] private Boolean m_InvertVertical;

		private void Update()
		{
			var right = MotionTarget.right;
			var forward = MotionTarget.forward;
			var moveDir = m_Sideways.Value * right + m_Forward.Value * forward;

			m_Vertical.Value += m_Gravity * Time.deltaTime;
			moveDir.y += m_Vertical.Value;

			// clamp all values that we are using
			m_Sideways.Validate();
			m_Forward.Validate();
			m_Tilt.Validate();
			m_Pan.Validate();

			// tilting goes to camera tracking target as we don't want our viewmodel to tilt, just the camera
			CameraTarget.localRotation = Quaternion.Euler(m_Tilt.Value, 0f, 0f);
			MotionTarget.localRotation = Quaternion.Euler(0f, m_Pan.Value, 0f);

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
			var lookDir = context.performed ? context.ReadValue<Vector2>() : Vector2.zero;
			m_Tilt.Value += lookDir.y * RotationSensitivity.y * Time.deltaTime * (m_InvertVertical ? 1f : -1f);
			m_Pan.Value += lookDir.x * RotationSensitivity.x * Time.deltaTime;
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
