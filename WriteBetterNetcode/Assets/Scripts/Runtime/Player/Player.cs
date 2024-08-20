// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerAvatar))]
	[RequireComponent(typeof(PlayerVars), typeof(PlayerServer), typeof(PlayerClient))]
	public sealed class Player : NetworkBehaviour
	{
		private PlayerAvatar m_Avatar;
		private PlayerClient m_ClientSide;
		private PlayerVars m_Vars;

		public Byte AvatarIndex { get => m_Vars.AvatarIndex; set => m_Vars.AvatarIndex = value; }

		private void Awake()
		{
			m_Avatar = GetComponent<PlayerAvatar>();
			m_ClientSide = GetComponent<PlayerClient>();
			m_Vars = GetComponent<PlayerVars>();
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			// apply initial value
			m_Avatar.SetAvatar(AvatarIndex);
		}

		public override void OnNetworkDespawn() => base.OnNetworkDespawn();

		internal void OnAvatarIndexChanged(Byte prevAvatarIndex, Byte avatarIndex) => m_Avatar.SetAvatar(avatarIndex);
	}
}
