// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	public sealed class PlayerInteraction : MonoBehaviour, IPlayerComponent, GeneratedInput.IPlayerInteractionActions
	{
		private Int32 m_PlayerIndex;

		private void Awake()
		{
		}

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;
			Debug.Log("spawn");

			var inputUsers = Components.InputUsers;
			inputUsers.SetPlayerInteractionCallback(playerIndex, this);
		}
		public void OnPlayerDespawn(Int32 playerIndex)
		{
			var inputUsers = Components.InputUsers;
			inputUsers.SetPlayerInteractionCallback(playerIndex, null);
		}

		public void OnAttack(InputAction.CallbackContext context)
		{
			Debug.Log("Attacking ...");

		}

		public void OnInteract(InputAction.CallbackContext context)
		{
			Debug.Log("Interacting ...");
		}


	}
}
