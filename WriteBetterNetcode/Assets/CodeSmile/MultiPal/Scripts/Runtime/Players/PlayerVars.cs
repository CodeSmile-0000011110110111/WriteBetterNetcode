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

		private Player m_Player;

		internal Byte AvatarIndex
		{
			get => m_AvatarIndexVar.Value;
			set
			{
				if (IsServer)
					m_AvatarIndexVar.Value = value;
				else
					Debug.LogWarning($"set not permitted: {nameof(AvatarIndex)}");
			}
		}

		public void OnPlayerSpawn(Int32 playerIndex) =>
			// invoke directly for initial value
			m_AvatarIndexVar.OnValueChanged.Invoke(m_AvatarIndexVar.Value, m_AvatarIndexVar.Value);

		public void OnPlayerDespawn(Int32 playerIndex) {}

		private void Awake() => m_Player = GetComponent<Player>();

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
