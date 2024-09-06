// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Utility;
using CodeSmile.MultiPal.Input;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Player
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerAvatar), typeof(PlayerController))]
	[RequireComponent(typeof(PlayerVars), typeof(PlayerServer), typeof(PlayerClient))]
	public sealed class Player : NetworkBehaviour, IPlayerComponent
	{
		public Action<Int32> OnRequestToggleIngameMenu;

		private PlayerAvatar m_Avatar;
		private PlayerClient m_ClientSide;
		private PlayerVars m_Vars;

		public Byte AvatarIndex { get => m_Vars.AvatarIndex; set => m_Vars.AvatarIndex = value; }

		public Int32 PlayerIndex { get; private set; } = -1;

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			PlayerIndex = playerIndex;

			foreach (var playerComponent in GetComponentsInChildren<IPlayerComponent>())
			{
				// don't infinite recurse this
				if (Equals(playerComponent))
					continue;

				// Log component execution order (same as order on Inspector
				//Debug.Log($"OnPlayerSpawn called for: {playerComponent.GetType().Name}");

				playerComponent.OnPlayerSpawn(playerIndex);
			}
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			foreach (var playerComponent in GetComponentsInChildren<IPlayerComponent>())
			{
				// don't infinite recurse this
				if (Equals(playerComponent))
					continue;

				playerComponent.OnPlayerDespawn(playerIndex);
			}
		}

		private void Awake()
		{
			m_Avatar = GetComponent<PlayerAvatar>();
			m_ClientSide = GetComponent<PlayerClient>();
			m_Vars = GetComponent<PlayerVars>();
		}

		internal void OnAvatarIndexChanged(Byte _, Byte avatarIndex) => m_Avatar.SetAvatar(avatarIndex);

		public void OnOpenIngameMenu()
		{
			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.AllPlayerInteractionEnabled = false;
			inputUsers.AllPlayerKinematicsEnabled = false;
			inputUsers.AllPlayerUiEnabled = false;
			inputUsers.AllUiEnabled = true;

			// leave request menu enabled to allow for quick dismissal
			inputUsers.SetPlayerUiRequestMenuEnabled(PlayerIndex, true);
		}

		public void OnCloseIngameMenu()
		{
			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.AllPlayerInteractionEnabled = true;
			inputUsers.AllPlayerKinematicsEnabled = true;
			inputUsers.AllPlayerUiEnabled = true;
			inputUsers.AllUiEnabled = false;
		}
	}
}
