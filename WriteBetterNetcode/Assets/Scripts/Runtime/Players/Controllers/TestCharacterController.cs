// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile
{
	public sealed class TestCharacterController : ModularCharacterControllerBase, GeneratedInputActions.IPlayerActions
	{
		private void Update()
		{
			m_CharacterController.Move(new Vector3(0f, 0.0012f, 0f));

			var pos = m_CharacterController.transform.localPosition;
			if (pos.y > 4f)
				m_CharacterController.transform.localPosition = new Vector3(pos.x, 0f, pos.z);
		}

		public void OnMove(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnLook(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnAttack(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnInteract(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnCrouch(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnJump(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnPrevious(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnNext(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnSprint(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnPause(InputAction.CallbackContext context) => throw new System.NotImplementedException();
	}
}
