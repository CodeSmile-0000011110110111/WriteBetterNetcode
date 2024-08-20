// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	internal class PlayerServer : NetworkBehaviour
	{
		private Player m_Player;

		private void Awake() => m_Player = GetComponent<Player>();
	}
}
