// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Extensions.Netcode;
using CodeSmile.MultiPal.Settings;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players.Couch
{
	[DisallowMultipleComponent]
	internal sealed class CouchPlayersClient : NetworkBehaviour
	{
		private readonly TaskCompletionSource<Player>[] m_SpawnTcs =
			new TaskCompletionSource<Player>[Constants.MaxCouchPlayers];

		private CouchPlayersServer m_ServerSide;

		private Boolean IsOffline => NetworkManagerExt.IsOffline;

		private void Awake()
		{
			m_ServerSide = GetComponent<CouchPlayersServer>();
		}

		internal async Task<Player> SpawnPlayer(Int32 playerIndex, Int32 avatarIndex)
		{
			if (m_SpawnTcs[playerIndex] != null)
				throw new Exception($"player {playerIndex} spawn in progress");

			m_SpawnTcs[playerIndex] = new TaskCompletionSource<Player>();
			m_ServerSide.SpawnPlayer(OwnerClientId, (Byte)playerIndex, (Byte)avatarIndex);

			var result = await m_SpawnTcs[playerIndex].Task;
			m_SpawnTcs[playerIndex] = null;

			return result;
		}

		internal void DidSpawnPlayer(NetworkObject playerObj, Byte playerIndex)
		{
			if (IsOffline)
				DidSpawnPlayerClientSide(playerObj, playerIndex);
			else
				DidSpawnPlayerClientRpc(playerObj, playerIndex);
		}

		private void DidSpawnPlayerClientSide(NetworkObject playerObj, Byte playerIndex)
		{
			var player = playerObj.GetComponent<Player>();

			// end awaitable task
			m_SpawnTcs[playerIndex].SetResult(player);
		}

		[Rpc(SendTo.Owner, DeferLocal = true)]
		private void DidSpawnPlayerClientRpc(NetworkObjectReference playerRef, Byte playerIndex)
		{
			// this should not fail thus no error check
			playerRef.TryGet(out var playerObj);

			DidSpawnPlayerClientSide(playerObj, playerIndex);
		}

		internal void DespawnPlayer(Int32 playerIndex, NetworkObject playerObj)
		{
			// Despawn may get invoked when session stopped, thus object may already be despawned
			if (playerObj.IsSpawned || IsOffline)
				m_ServerSide.DespawnPlayer((Byte)playerIndex, playerObj);
		}
	}
}
