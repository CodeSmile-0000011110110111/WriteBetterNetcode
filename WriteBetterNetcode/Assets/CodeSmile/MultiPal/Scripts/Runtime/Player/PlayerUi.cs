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
	public sealed class PlayerUi : MonoBehaviour, IPlayerComponent, GeneratedInput.IPlayerUIActions
	{
		private Int32 m_PlayerIndex;

		private Player m_Player;
		private PlayerAvatar m_Avatar;
		private PlayerController m_Controller;

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;

			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.SetPlayerUiCallback(playerIndex, this);
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.SetPlayerUiCallback(playerIndex, null);
		}

		public void OnRequestMenu(InputAction.CallbackContext context)
		{
			if (context.performed)
				m_Player.RequestToggleIngameMenu(m_PlayerIndex);
		}

		public void OnPrevious(InputAction.CallbackContext context)
		{
			if (context.performed)
				m_Player.AvatarIndex = m_Avatar.PreviousIndex;
		}

		public void OnNext(InputAction.CallbackContext context)
		{
			if (context.performed)
				m_Player.AvatarIndex = m_Avatar.NextIndex;
		}

		public void OnUp(InputAction.CallbackContext context)
		{
			if (context.performed)
				m_Controller.NextController();
		}

		public void OnDown(InputAction.CallbackContext context)
		{
			if (context.performed)
				m_Controller.PreviousController();
		}

		private void Awake()
		{
			m_Player = GetComponent<Player>();
			m_Avatar = GetComponent<PlayerAvatar>();
			m_Controller = GetComponent<PlayerController>();
		}
	}
}
