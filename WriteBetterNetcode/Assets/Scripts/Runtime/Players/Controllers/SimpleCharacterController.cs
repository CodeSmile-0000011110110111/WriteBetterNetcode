// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile
{
	public sealed class SimpleCharacterController : ModularCharacterControllerBase
	{
		[SerializeField] private Single m_Gravity = -1f;

		private void Update()
		{
			AddVerticalVelocity(m_Gravity);
			Move();
		}

		public override void OnMove(InputAction.CallbackContext context)
		{
			var moveDir = GetHorizontalVelocity(context);
			moveDir.x *= MotionSensitivity.x;
			moveDir.y *= MotionSensitivity.z;
			SetHorizontalVelocity(moveDir);
		}

		public override void OnLook(InputAction.CallbackContext context) {}
		public override void OnAttack(InputAction.CallbackContext context) {}
		public override void OnInteract(InputAction.CallbackContext context) {}
		public override void OnCrouch(InputAction.CallbackContext context) {}

		public override void OnJump(InputAction.CallbackContext context)
		{
			if (context.performed)
				SetVerticalVelocity(1f * MotionSensitivity.y);
		}

		public override void OnPrevious(InputAction.CallbackContext context) {}
		public override void OnNext(InputAction.CallbackContext context) {}
		public override void OnSprint(InputAction.CallbackContext context) {}
	}
}
