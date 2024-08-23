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
		private readonly NetworkVariable<Byte> m_AvatarIndexVar = new(Byte.MaxValue);

		private Player m_Player;

		internal Byte AvatarIndex
		{
			get => m_AvatarIndexVar.Value;
			set
			{
				if (IsServer)
				{
					SetAvatarIndexServerRpc(value);
				}
				else
					Debug.LogWarning($"set not permitted: {nameof(AvatarIndex)}");
			}
		}

		private void Awake() => m_Player = GetComponent<Player>();

		[Rpc(SendTo.Server, DeferLocal = true)]
		private void SetAvatarIndexServerRpc(Byte avatarIndex)
		{
			// apply locally directly
			//m_AvatarIndexVar.OnValueChanged.Invoke(m_AvatarIndexVar.Value, avatarIndex);
			m_AvatarIndexVar.Value = avatarIndex;
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			m_AvatarIndexVar.OnValueChanged += m_Player.OnAvatarIndexChanged;

			// invoke directly for initial value
			m_AvatarIndexVar.OnValueChanged.Invoke(m_AvatarIndexVar.Value, m_AvatarIndexVar.Value);
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			m_AvatarIndexVar.OnValueChanged -= m_Player.OnAvatarIndexChanged;
		}
	}
}
