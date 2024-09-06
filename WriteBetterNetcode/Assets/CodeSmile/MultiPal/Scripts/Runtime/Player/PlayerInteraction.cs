// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using CodeSmile.Components.Utility;
using CodeSmile.MultiPal.Input;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.MultiPal.Player
{
	[DisallowMultipleComponent]
	public sealed class PlayerInteraction : MonoBehaviour, IPlayerComponent, GeneratedInput.IPlayerInteractionActions
	{
		private Int32 m_PlayerIndex;
		private Player m_Player;

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;

			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.SetPlayerInteractionCallback(playerIndex, this);
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.SetPlayerInteractionCallback(playerIndex, null);
		}

		public void OnAttack(InputAction.CallbackContext context) {}

		public void OnInteract(InputAction.CallbackContext context)
		{
			if (context.performed)
				m_Player.SwitchCamera();
		}

		private void Awake() => m_Player = GetComponent<Player>();
	}
}
