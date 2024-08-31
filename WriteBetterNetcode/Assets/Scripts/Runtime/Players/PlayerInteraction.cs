// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	public sealed class PlayerInteraction : MonoBehaviour, IPlayerComponent, GeneratedInput.IPlayerInteractionActions
	{
		private Int32 m_PlayerIndex;
		private PlayerCamera m_PlayerCamera;

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;

			var inputUsers = Components.InputUsers;
			inputUsers.SetPlayerInteractionCallback(playerIndex, this);
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			var inputUsers = Components.InputUsers;
			inputUsers.SetPlayerInteractionCallback(playerIndex, null);
		}

		public void OnAttack(InputAction.CallbackContext context) {}

		public void OnInteract(InputAction.CallbackContext context)
		{
			Debug.Log($"interact! performed: {context.performed}");
			if (context.performed)
				m_PlayerCamera.NextCamera();
		}

		private void Awake() => m_PlayerCamera = GetComponent<PlayerCamera>();
	}
}
