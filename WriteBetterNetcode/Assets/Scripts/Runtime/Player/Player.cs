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
	[RequireComponent(typeof(PlayerServer), typeof(PlayerClient))]
	[RequireComponent(typeof(PlayerServerVars), typeof(PlayerOwnerVars))]
	public sealed class Player : NetworkBehaviour
	{
		private PlayerAvatar m_Avatar;

		private PlayerClient m_Client;

		private PlayerServerVars m_ServerVars;
		private PlayerOwnerVars m_OwnerVars;

		public Byte AvatarIndex { get => m_ServerVars.AvatarIndex; set => m_ServerVars.AvatarIndex = value; }

		private void Awake()
		{
			m_Avatar = GetComponent<PlayerAvatar>();
			m_Client = GetComponent<PlayerClient>();
			m_ServerVars = GetComponent<PlayerServerVars>();
			m_OwnerVars = GetComponent<PlayerOwnerVars>();
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
