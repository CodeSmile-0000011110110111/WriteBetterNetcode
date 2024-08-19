// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Game
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ServerPlayerSpawner))]
	public sealed class ClientPlayerSpawner : NetworkBehaviour
	{
		private readonly TaskCompletionSource<LocalPlayer>[] m_SpawnTcs =
			new TaskCompletionSource<LocalPlayer>[LocalPlayers.MaxLocalPlayers];

		private ServerPlayerSpawner m_ServerPlayerSpawner;

		private void Awake() => m_ServerPlayerSpawner = GetComponent<ServerPlayerSpawner>();

		public Task<LocalPlayer> Spawn(Int32 localPlayerIndex, Int32 prefabIndex)
		{
			m_SpawnTcs[localPlayerIndex] = new TaskCompletionSource<LocalPlayer>();
			m_ServerPlayerSpawner.SpawnPlayerServerRpc(OwnerClientId, localPlayerIndex, prefabIndex);
			return m_SpawnTcs[localPlayerIndex].Task;
		}

		[Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
		internal void DidSpawnPlayerClientRpc(NetworkObjectReference playerObjectRef, Int32 localPlayerIndex)
		{
			if (IsOwner)
			{
				var net = NetworkManager.Singleton;
				var player = net.SpawnManager.SpawnedObjects[playerObjectRef.NetworkObjectId];
				m_SpawnTcs[localPlayerIndex].SetResult(player.GetComponent<LocalPlayer>());
				m_SpawnTcs[localPlayerIndex] = null;
			}
		}
	}
}
