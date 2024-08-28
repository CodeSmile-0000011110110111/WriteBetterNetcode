// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Player
{
	public class PlayerInputActions : MonoBehaviour, GeneratedInputActions.IPlayerActions
	{
		private Player m_Player;

		// handle player-specific input
		public void OnMove(InputAction.CallbackContext context)
		{
			if (context.performed)
				Debug.Log($"Move: Player #{m_Player.PlayerIndex}");
		}

		public void OnLook(InputAction.CallbackContext context)
		{
			if (context.performed)
				Debug.Log($"Look: Player #{m_Player.PlayerIndex}");
		}

		public void OnAttack(InputAction.CallbackContext context)
		{
			if (context.performed)
				Debug.Log($"Attack: Player #{m_Player.PlayerIndex}");
		}

		public void OnInteract(InputAction.CallbackContext context)
		{
			if (context.performed)
				Debug.Log($"Interact: Player #{m_Player.PlayerIndex}");
		}

		public void OnCrouch(InputAction.CallbackContext context)
		{
			if (context.performed)
				Debug.Log($"Crouch: Player #{m_Player.PlayerIndex}");
		}

		public void OnJump(InputAction.CallbackContext context)
		{
			if (context.performed)
				Debug.Log($"Jump: Player #{m_Player.PlayerIndex}");
		}

		public void OnPrevious(InputAction.CallbackContext context)
		{
			if (context.performed)
				Debug.Log($"Previous: Player #{m_Player.PlayerIndex}");
		}

		public void OnNext(InputAction.CallbackContext context)
		{
			if (context.performed)
				Debug.Log($"Next: Player #{m_Player.PlayerIndex}");
		}

		public void OnSprint(InputAction.CallbackContext context)
		{
			if (context.performed)
				Debug.Log($"Sprint: Player #{m_Player.PlayerIndex}");
		}

		private void Awake() => m_Player = GetComponent<Player>();

		public void RegisterCallback(Int32 playerIndex)
		{
			Debug.Log("PlayerInputActions REGISTER");
			var inputActions = Components.InputUsers.Actions[playerIndex];
			inputActions.Player.SetCallbacks(this);
			inputActions.Player.Enable();
		}

		public void UnregisterCallback(Int32 playerIndex)
		{
			Debug.Log("PlayerInputActions UNREGISTER");
			var inputActions = Components.InputUsers.Actions[playerIndex];
			inputActions.Player.Disable();
			inputActions.Player.SetCallbacks(null);
		}
	}
}
