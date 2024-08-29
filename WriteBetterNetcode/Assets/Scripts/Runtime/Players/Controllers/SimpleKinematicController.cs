// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Input;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Players.Controllers
{
	public sealed class SimpleKinematicController : KinematicControllerBase
	{
		[SerializeField] private Single m_Gravity = -1f;

		private void Update()
		{
			AddVerticalVelocity(m_Gravity);
			Move();
		}

		public override void OnMove(InputAction.CallbackContext context)
		{
			if (InputUsers.GetUserIndex(context) == PlayerIndex)
			{
				var moveDir = GetHorizontalVelocity(context);
				moveDir.x *= MotionSensitivity.x;
				moveDir.y *= MotionSensitivity.z;
				SetHorizontalVelocity(moveDir);
			}
		}

		public override void OnLook(InputAction.CallbackContext context) {}
		public override void OnCrouch(InputAction.CallbackContext context) {}

		public override void OnJump(InputAction.CallbackContext context)
		{
			if (InputUsers.GetUserIndex(context) == PlayerIndex)
			{
				if (context.performed)
					SetVerticalVelocity(1f * MotionSensitivity.y);
			}
		}

		public override void OnSprint(InputAction.CallbackContext context) {}
	}
}
