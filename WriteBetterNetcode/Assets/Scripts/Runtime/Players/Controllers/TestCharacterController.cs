// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile
{
	public sealed class TestCharacterController : ModularCharacterControllerBase
	{
		private void Update()
		{
			m_CharacterController.Move(new Vector3(0f, 0.0012f, 0f));

			var pos = m_CharacterController.transform.localPosition;
			if (pos.y > 4f)
				m_CharacterController.transform.localPosition = new Vector3(pos.x, 0f, pos.z);
		}

		public override void OnMove(InputAction.CallbackContext context)
		{
			if (context.performed)
				Debug.Log("OnMove");
		}

		public override void OnLook(InputAction.CallbackContext context) => throw new NotImplementedException();
		public override void OnAttack(InputAction.CallbackContext context) => throw new NotImplementedException();
		public override void OnInteract(InputAction.CallbackContext context) => throw new NotImplementedException();
		public override void OnCrouch(InputAction.CallbackContext context) => throw new NotImplementedException();
		public override void OnJump(InputAction.CallbackContext context) => throw new NotImplementedException();
		public override void OnPrevious(InputAction.CallbackContext context) => throw new NotImplementedException();
		public override void OnNext(InputAction.CallbackContext context) => throw new NotImplementedException();
		public override void OnSprint(InputAction.CallbackContext context) => throw new NotImplementedException();
	}
}
