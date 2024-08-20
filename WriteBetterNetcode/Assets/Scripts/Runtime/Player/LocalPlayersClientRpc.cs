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
	internal sealed class LocalPlayersClientRpc : NetworkBehaviour
	{
		private readonly TaskCompletionSource<Player>[] m_SpawnTcs =
			new TaskCompletionSource<Player>[LocalPlayers.MaxLocalPlayers];

		private LocalPlayersServerRpc m_ServerRpc;

		private void Awake() => m_ServerRpc = GetComponent<LocalPlayersServerRpc>();

		public Task<Player> Spawn(Int32 localPlayerIndex, Int32 avatarIndex)
		{
			if (m_SpawnTcs[localPlayerIndex] != null)
				throw new Exception($"spawn already in progress, player index: {localPlayerIndex}");

			m_SpawnTcs[localPlayerIndex] = new TaskCompletionSource<Player>();
			m_ServerRpc.SpawnPlayerServerRpc(localPlayerIndex, (byte)avatarIndex, OwnerClientId);
			return m_SpawnTcs[localPlayerIndex].Task;
		}

		[Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
		internal void DidSpawnPlayerClientRpc(NetworkObjectReference playerRef, Int32 localPlayerIndex, byte avatarIndex)
		{
			if (IsOwner)
			{
				// this should not fail thus no error check
				playerRef.TryGet(out var playerObj);

				var localPlayer = playerObj.GetComponent<Player>();
				localPlayer.AvatarIndex = avatarIndex;

				// end awaitable task, and discard
				m_SpawnTcs[localPlayerIndex].SetResult(localPlayer);
				m_SpawnTcs[localPlayerIndex] = null;
			}
		}
	}
}
