// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	internal sealed class CouchPlayersServer : NetworkBehaviour
	{
		[SerializeField] private NetworkObject m_PlayerPrefab;

		private CouchPlayersClient m_Client;

		private void Awake()
		{
			if (m_PlayerPrefab == null)
				throw new MissingReferenceException(nameof(m_PlayerPrefab));

			m_Client = GetComponent<CouchPlayersClient>();
		}

		[Rpc(SendTo.Server, DeferLocal = true)]
		internal void SpawnPlayerServerRpc(Byte localPlayerIndex, Byte avatarIndex, UInt64 ownerId)
		{
			var playerObj = Instantiate(m_PlayerPrefab).GetComponent<NetworkObject>();
			playerObj.SpawnWithOwnership(ownerId);

			var player = playerObj.GetComponent<Player>();
			player.AvatarIndex = avatarIndex;

			m_Client.DidSpawnPlayerClientRpc(playerObj, localPlayerIndex, avatarIndex);
		}
	}
}
