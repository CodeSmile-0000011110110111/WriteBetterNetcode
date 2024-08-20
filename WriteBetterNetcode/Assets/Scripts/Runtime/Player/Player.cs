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
	public sealed class Player : NetworkBehaviour
	{
		private OwnerVars m_OwnerVars;
		private PlayerAvatar m_Avatar;

		public Int32 AvatarIndex { get => m_OwnerVars.AvatarIndex.Value; set => m_OwnerVars.AvatarIndex.Value = (Byte)value; }

		private void Awake()
		{
			m_Avatar = GetComponent<PlayerAvatar>();

			m_OwnerVars = new OwnerVars(this);
			m_OwnerVars.AvatarIndex.OnValueChanged += OnAvatarIndexChanged;

			// apply initial value
			m_Avatar.OnAvatarIndexChanged(0, (Byte)AvatarIndex);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			m_OwnerVars.UnregisterAllCallbacks();
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			// if (IsOwner == false)
			// {
			// 	var playerObj = NetworkManager.SpawnManager.GetLocalPlayerObject();
			// 	var localPlayers = playerObj.GetComponent<LocalPlayers>();
			// 	localPlayers.OnRemotePlayerSpawn(this);
			// }
		}

		private void OnAvatarIndexChanged(Byte prevAvatarIndex, Byte avatarIndex) =>
			m_Avatar.OnAvatarIndexChanged(prevAvatarIndex, avatarIndex);

		private sealed class OwnerVars
		{
			public readonly NetworkVariable<Byte> AvatarIndex = new(writePerm: NetworkVariableWritePermission.Owner);

			public OwnerVars(Player player) =>
				// must call Initialize because the vars are not in a NetworkBehaviour class
				AvatarIndex.Initialize(player);

			public void UnregisterAllCallbacks() => AvatarIndex.OnValueChanged = null;
		}
	}
}
