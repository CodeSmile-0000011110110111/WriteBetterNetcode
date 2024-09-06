// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Settings;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Player
{
	[DisallowMultipleComponent]
	internal sealed class CouchPlayersClient : NetworkBehaviour
	{
		private readonly TaskCompletionSource<Player>[] m_SpawnTcs =
			new TaskCompletionSource<Player>[Constants.MaxCouchPlayers];

		private CouchPlayers m_Players;
		private CouchPlayersServer m_ServerSide;

		private void Awake()
		{
			m_Players = GetComponent<CouchPlayers>();
			m_ServerSide = GetComponent<CouchPlayersServer>();
		}

		internal Task<Player> Spawn(Vector3 position, Int32 playerIndex, Int32 avatarIndex)
		{
			if (m_SpawnTcs[playerIndex] != null)
				throw new Exception($"player {playerIndex} spawn in progress");

			m_ServerSide.SpawnPlayerServerRpc(OwnerClientId, position, (Byte)playerIndex, (Byte)avatarIndex);

			m_SpawnTcs[playerIndex] = new TaskCompletionSource<Player>();
			return m_SpawnTcs[playerIndex].Task;
		}

		[Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
		internal void DidSpawnPlayerClientRpc(NetworkObjectReference playerRef, Byte playerIndex)
		{
			// this should not fail thus no error check
			playerRef.TryGet(out var playerObj);

			var player = playerObj.GetComponent<Player>();

			if (IsOwner)
			{
				// end awaitable task, and discard
				m_SpawnTcs[playerIndex].SetResult(player);
				m_SpawnTcs[playerIndex] = null;
			}
			else
				m_Players.AddRemotePlayer(player, playerIndex);
		}

		internal void Despawn(NetworkObject playerObj)
		{
			// Despawn may get invoked when session stopped, thus object may already be despawned
			if (playerObj.IsSpawned)
				m_ServerSide.DespawnPlayerServerRpc(playerObj);
		}
	}
}
