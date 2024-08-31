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
	public sealed class PlayerUi : MonoBehaviour, IPlayerComponent, GeneratedInput.IPlayerUIActions
	{
		private Int32 m_PlayerIndex;

		private void Awake()
		{
		}

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;

			var inputUsers = Components.InputUsers;
			inputUsers.SetPlayerUiCallback(playerIndex, this);
		}
		public void OnPlayerDespawn(Int32 playerIndex)
		{
			var inputUsers = Components.InputUsers;
			inputUsers.SetPlayerUiCallback(playerIndex, null);
		}

		public void OnRequestMenu(InputAction.CallbackContext context)
		{
			Debug.Log("menu request (not handled)");
		}
		public void OnPrevious(InputAction.CallbackContext context){}
		public void OnNext(InputAction.CallbackContext context) {}
		public void OnUp(InputAction.CallbackContext context) {}
		public void OnDown(InputAction.CallbackContext context){}
	}
}
