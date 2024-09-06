// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Player
{
	[DisallowMultipleComponent]
	internal class PlayerServer : NetworkBehaviour
	{
		private PlayerClient m_ClientSide;
		private Player m_Player;

		private void Awake()
		{
			m_ClientSide = GetComponent<PlayerClient>();
			m_Player = GetComponent<Player>();
		}
	}
}
