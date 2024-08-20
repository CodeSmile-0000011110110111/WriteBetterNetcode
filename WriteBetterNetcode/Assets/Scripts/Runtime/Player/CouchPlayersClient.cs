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
			new TaskCompletionSource<Player>[CouchPlayers.MaxLocalPlayers];

		private CouchPlayers m_Players;
		private CouchPlayersServer m_Server;

		private void Awake()
		{
			m_Players = GetComponent<CouchPlayers>();
			m_Server = GetComponent<CouchPlayersServer>();
		}

		public Task<Player> Spawn(Int32 localPlayerIndex, Int32 avatarIndex)
		{
			if (m_SpawnTcs[localPlayerIndex] != null)
				throw new Exception($"spawn already in progress, player index: {localPlayerIndex}");

			m_SpawnTcs[localPlayerIndex] = new TaskCompletionSource<Player>();
			m_Server.SpawnPlayerServerRpc((Byte)localPlayerIndex, (Byte)avatarIndex, OwnerClientId);
			return m_SpawnTcs[localPlayerIndex].Task;
		}

		[Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
		internal void DidSpawnPlayerClientRpc(NetworkObjectReference playerRef, Byte localPlayerIndex, Byte avatarIndex)
		{
			// this should not fail thus no error check
			playerRef.TryGet(out var playerObj);

			var player = playerObj.GetComponent<Player>();
			// player.AvatarIndex = avatarIndex;

			if (IsOwner)
			{
				// end awaitable task, and discard
				m_SpawnTcs[localPlayerIndex].SetResult(player);
				m_SpawnTcs[localPlayerIndex] = null;
			}
			else
			{
				m_Players.RegisterRemotePlayer(player, localPlayerIndex);
			}
		}
	}
}
