// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	internal sealed class PlayerServerVars : NetworkBehaviour
	{
		private readonly NetworkVariable<Byte> m_AvatarIndexVar = new();
		private Player m_Player;

		internal Byte AvatarIndex
		{
			get => m_AvatarIndexVar.Value;
			set
			{
				if (IsServer)
					m_AvatarIndexVar.Value = value;
				else
					SetAvatarIndexServerRpc(value);
			}
		}

		private void Awake() => m_Player = GetComponent<Player>();

		[Rpc(SendTo.Server, DeferLocal = true)]
		private void SetAvatarIndexServerRpc(Byte avatarIndex) => m_AvatarIndexVar.Value = avatarIndex;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			m_AvatarIndexVar.OnValueChanged += m_Player.OnAvatarIndexChanged;
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			m_AvatarIndexVar.OnValueChanged -= m_Player.OnAvatarIndexChanged;
		}
	}
}
