﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerAvatar), typeof(PlayerInputActions))]
	[RequireComponent(typeof(PlayerVars), typeof(PlayerServer), typeof(PlayerClient))]
	public sealed class Player : NetworkBehaviour
	{
		private PlayerAvatar m_Avatar;
		private PlayerClient m_ClientSide;
		private PlayerVars m_Vars;

		public Byte AvatarIndex { get => m_Vars.AvatarIndex; set => m_Vars.AvatarIndex = value; }

		public int PlayerIndex { get; private set; } = -1;

		private void Awake()
		{
			m_Avatar = GetComponent<PlayerAvatar>();
			m_ClientSide = GetComponent<PlayerClient>();
			m_Vars = GetComponent<PlayerVars>();
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			OnCouchPlayerDespawn();
		}

		internal void OnAvatarIndexChanged(Byte _, Byte avatarIndex) => m_Avatar.SetAvatar(avatarIndex);

		public void OnCouchPlayerSpawned(int playerIndex)
		{
			PlayerIndex = playerIndex;

			var input = GetComponent<PlayerInputActions>();
			input.RegisterCallback(PlayerIndex);
		}

		private void OnCouchPlayerDespawn()
		{
			var input = GetComponent<PlayerInputActions>();
			input.UnregisterCallback(PlayerIndex);

			PlayerIndex = -1;
		}

	}
}
