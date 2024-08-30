// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
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
		public event Action<Int32> DidRequestToggleIngameMenu;

		// FIXME: we should receive a callback from GuiController instead
		public static Boolean m_IsIngameMenuOpen;

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

			var inputUsers = Components.InputUsers;
			inputUsers.SetPlayerUiCallback(playerIndex, this);
			inputUsers.SetPlayerKinematicsCallback(playerIndex, GetComponent<PlayerKinematics>());
			//inputUsers.LogActionEnabledness($"Player {playerIndex} Spawn:\n");
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			var inputUsers = Components.InputUsers;
			inputUsers.SetPlayerUiCallback(playerIndex, null);
			inputUsers.SetPlayerKinematicsCallback(playerIndex, null);
			//inputUsers.LogActionEnabledness($"Player {playerIndex} Despawn:\n");

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
				DidRequestToggleIngameMenu?.Invoke(PlayerIndex);
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

		internal void OnAvatarIndexChanged(Byte _, Byte avatarIndex) => m_Avatar.SetAvatar(avatarIndex);

		public void OnOpenIngameMenu()
		{
			var inputUsers = Components.InputUsers;
			inputUsers.AllPlayerInteractionEnabled = false;
			inputUsers.AllPlayerKinematicsEnabled = false;
			inputUsers.AllUiEnabled = true;
		}

		public void OnCloseIngameMenu()
		{
			var inputUsers = Components.InputUsers;
			inputUsers.AllPlayerInteractionEnabled = true;
			inputUsers.AllPlayerKinematicsEnabled = true;
			inputUsers.AllUiEnabled = false;
		}
	}
}
