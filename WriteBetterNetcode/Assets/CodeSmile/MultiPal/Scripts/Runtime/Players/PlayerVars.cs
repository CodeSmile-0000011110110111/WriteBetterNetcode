// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players
{
	[DisallowMultipleComponent]
	internal sealed class PlayerVars : NetworkBehaviour, IPlayerComponent
	{
		private readonly NetworkVariable<Byte> m_AvatarIndexVar = new();
		private readonly NetworkVariable<Single> m_Hitpoints = new();

		private Player m_Player;
		private PlayerAvatar m_Avatar;

		internal Byte AvatarIndex
		{
			get => m_AvatarIndexVar.Value;
			set => AvatarIndexChangeServerRpc(value);
		}

		public void OnPlayerSpawn(Int32 playerIndex, Boolean isOwner) {}

		public void OnPlayerDespawn(Int32 playerIndex, Boolean isOwner) {}

		private void Awake()
		{
			m_Player = GetComponent<Player>();
			m_Avatar = GetComponent<PlayerAvatar>();
		}

		[Rpc(SendTo.Server)]
		private void AvatarIndexChangeServerRpc(Byte avatarIndex) => m_AvatarIndexVar.Value = avatarIndex;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			m_AvatarIndexVar.OnValueChanged += m_Avatar.OnAvatarIndexChanged;
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			m_AvatarIndexVar.OnValueChanged -= m_Avatar.OnAvatarIndexChanged;
		}
	}
}
