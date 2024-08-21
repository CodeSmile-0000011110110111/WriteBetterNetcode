// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	internal sealed class CouchPlayersClient : NetworkBehaviour
	{
		private readonly TaskCompletionSource<Player>[] m_SpawnTcs =
			new TaskCompletionSource<Player>[CouchPlayers.MaxCouchPlayers];

		private CouchPlayers m_Players;
		private CouchPlayersServer m_ServerSide;

		private void Awake()
		{
			m_Players = GetComponent<CouchPlayers>();
			m_ServerSide = GetComponent<CouchPlayersServer>();
		}

		internal Task<Player> Spawn(Int32 couchPlayerIndex, Int32 avatarIndex)
		{
			if (m_SpawnTcs[couchPlayerIndex] != null)
				throw new Exception($"player {couchPlayerIndex} spawn in progress");

			m_ServerSide.SpawnPlayerServerRpc(OwnerClientId,
				(Byte)couchPlayerIndex, (Byte)avatarIndex);

			m_SpawnTcs[couchPlayerIndex] = new TaskCompletionSource<Player>();
			return m_SpawnTcs[couchPlayerIndex].Task;
		}

		[Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
		internal void DidSpawnPlayerClientRpc(NetworkObjectReference playerRef,
			Byte couchPlayerIndex, byte avatarIndex)
		{
			// this should not fail thus no error check
			playerRef.TryGet(out var playerObj);

			var player = playerObj.GetComponent<Player>();

			if (IsOwner)
			{
				player.AvatarIndex = avatarIndex;

				// end awaitable task, and discard
				m_SpawnTcs[couchPlayerIndex].SetResult(player);
				m_SpawnTcs[couchPlayerIndex] = null;
			}
			else
				m_Players.AddRemotePlayer(player, couchPlayerIndex);
		}
	}
}
