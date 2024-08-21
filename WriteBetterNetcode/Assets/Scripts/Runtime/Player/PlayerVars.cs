// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	internal sealed class PlayerVars : NetworkBehaviour
	{
		private readonly NetworkVariable<Byte> m_AvatarIndexVar =
			new(writePerm: NetworkVariableWritePermission.Owner);
		private Player m_Player;

		internal Byte AvatarIndex
		{
			get => m_AvatarIndexVar.Value;
			set
			{
				if (IsOwner)
					SetAvatarIndexOwnerRpc(value);
				else
					Debug.LogWarning($"set not permitted: {nameof(AvatarIndex)}");
			}
		}

		private void Awake() => m_Player = GetComponent<Player>();

		[Rpc(SendTo.Owner, DeferLocal = true)]
		private void SetAvatarIndexOwnerRpc(Byte avatarIndex) => m_AvatarIndexVar.Value = avatarIndex;

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
