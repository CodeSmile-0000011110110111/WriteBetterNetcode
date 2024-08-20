// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	internal sealed class LocalPlayersServerRpc : NetworkBehaviour
	{
		[SerializeField] private NetworkObject m_PlayerPrefab;

		private LocalPlayersClientRpc m_ClientRpc;

		private void Awake()
		{
			if (m_PlayerPrefab == null)
				throw new MissingReferenceException(nameof(m_PlayerPrefab));

			m_ClientRpc = GetComponent<LocalPlayersClientRpc>();
		}

		[Rpc(SendTo.Server, DeferLocal = true)]
		internal void SpawnPlayerServerRpc(Int32 localPlayerIndex, byte avatarIndex, UInt64 ownerId)
		{
			var playerObj = Instantiate(m_PlayerPrefab).GetComponent<NetworkObject>();
			playerObj.SpawnWithOwnership(ownerId);

			m_ClientRpc.DidSpawnPlayerClientRpc(playerObj, localPlayerIndex, avatarIndex);
		}
	}
}
