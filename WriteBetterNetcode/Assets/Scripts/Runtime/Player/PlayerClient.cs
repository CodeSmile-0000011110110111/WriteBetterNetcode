// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	internal class PlayerClient : NetworkBehaviour
	{
		private PlayerServer m_Server;

		private void Awake() => m_Server = GetComponent<PlayerServer>();
	}
}
