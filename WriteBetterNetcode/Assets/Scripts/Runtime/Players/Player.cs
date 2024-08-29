// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerAvatar), typeof(PlayerKinematics))]
	[RequireComponent(typeof(PlayerVars), typeof(PlayerServer), typeof(PlayerClient))]
	public sealed class Player : NetworkBehaviour, IPlayerComponent
	{
		private PlayerAvatar m_Avatar;
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
				if (playerComponent != this)
					playerComponent.OnPlayerSpawn(playerIndex);
			}
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			foreach (var playerComponent in GetComponents<IPlayerComponent>())
			{
				// don't infinite recurse this
				if (playerComponent != this)
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
	}
}
