// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using CodeSmile.Input;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerAvatar), typeof(PlayerKinematics))]
	[RequireComponent(typeof(PlayerVars), typeof(PlayerServer), typeof(PlayerClient))]
	public sealed class Player : NetworkBehaviour, IPlayerComponent, GeneratedInput.IPlayerUIActions
	{
		public event Action<Int32> DidRequestMenu;

		private PlayerAvatar m_Avatar;
		private PlayerKinematics m_Kinematics;
		private PlayerClient m_ClientSide;
		private PlayerVars m_Vars;

		public Byte AvatarIndex { get => m_Vars.AvatarIndex; set => m_Vars.AvatarIndex = value; }

		public Int32 PlayerIndex { get; private set; } = -1;

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			PlayerIndex = playerIndex;

			foreach (var playerComponent in GetComponents<IPlayerComponent>())
			{
				// don't infinite recurse this
				if (Equals(playerComponent))
					continue;

				playerComponent.OnPlayerSpawn(playerIndex);
			}

			SetInputEnabled(true);
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			SetInputEnabled(false);

			foreach (var playerComponent in GetComponents<IPlayerComponent>())
			{
				// don't infinite recurse this
				if (Equals(playerComponent))
					continue;

				playerComponent.OnPlayerDespawn(playerIndex);
			}
		}

		public void OnRequestMenu(InputAction.CallbackContext context)
		{
			if (context.performed)
				DidRequestMenu?.Invoke(InputUsers.GetUserIndex(context));
		}

		public void OnPrevious(InputAction.CallbackContext context)
		{
			if (context.performed)
				AvatarIndex = m_Avatar.PreviousIndex;
		}

		public void OnNext(InputAction.CallbackContext context)
		{
			if (context.performed)
				AvatarIndex = m_Avatar.NextIndex;
		}

		public void OnUp(InputAction.CallbackContext context)
		{
			if (context.performed)
				m_Kinematics.NextController();
		}

		public void OnDown(InputAction.CallbackContext context)
		{
			if (context.performed)
				m_Kinematics.PreviousController();
		}

		private void Awake()
		{
			m_Avatar = GetComponent<PlayerAvatar>();
			m_Kinematics = GetComponent<PlayerKinematics>();
			m_ClientSide = GetComponent<PlayerClient>();
			m_Vars = GetComponent<PlayerVars>();
		}

		private void SetInputEnabled(Boolean enabled)
		{
			var inputUsers = Components.InputUsers;
			var actions = inputUsers.Actions[PlayerIndex];
			var playerUi = actions.PlayerUI;
			if (enabled)
			{
				playerUi.Enable();
				playerUi.SetCallbacks(this);
			}
			else
			{
				playerUi.Disable();
				playerUi.SetCallbacks(null);
			}
		}

		internal void OnAvatarIndexChanged(Byte _, Byte avatarIndex) => m_Avatar.SetAvatar(avatarIndex);
	}
}
