// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	[DisallowMultipleComponent]
	public sealed class LocalPlayerSpawner : NetworkBehaviour
	{
		private readonly List<LocalPlayer> m_ServerPlayerPrefabs = new();

		private readonly TaskCompletionSource<LocalPlayer>[] m_SpawnLocalPlayerTcs =
			new TaskCompletionSource<LocalPlayer>[LocalPlayers.MaxLocalPlayers];

		//private void Awake() => FindSpawnablePlayers();

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsServer)
				FindSpawnablePlayers();
		}

		private void FindSpawnablePlayers()
		{
			var net = NetworkManager.Singleton;
			foreach (var netPrefab in net.NetworkConfig.Prefabs.Prefabs)
			{
				if (netPrefab.Prefab.TryGetComponent<LocalPlayer>(out var localPlayerPrefab))
					m_ServerPlayerPrefabs.Add(localPlayerPrefab);
			}

			if (m_ServerPlayerPrefabs.Count == 0)
				throw new NetworkConfigurationException("could not find a LocalPlayer in NetworkPrefabs list!");
		}

		public Task<LocalPlayer> SpawnWithLocalPlayerIndex(Int32 localPlayerIndex)
		{
			if (m_SpawnLocalPlayerTcs[localPlayerIndex] != null)
				throw new Exception();

			m_SpawnLocalPlayerTcs[localPlayerIndex] = new TaskCompletionSource<LocalPlayer>();

			var prefabIndex = localPlayerIndex;
			SpawnPlayerServerRpc(OwnerClientId, localPlayerIndex, prefabIndex);

			return m_SpawnLocalPlayerTcs[localPlayerIndex].Task;
		}

		[Rpc(SendTo.Server, DeferLocal = true)]
		private void SpawnPlayerServerRpc(UInt64 ownerId, Int32 localPlayerIndex, Int32 prefabIndex)
		{
			try
			{
				var netObject = Instantiate(m_ServerPlayerPrefabs[prefabIndex]).GetComponent<NetworkObject>();
				netObject.SpawnAsPlayerObject(ownerId);

				DidSpawnPlayerClientRpc(netObject, localPlayerIndex);

			}
			catch (Exception e)
			{
				Debug.LogError(e);
				FailedSpawnPlayerClientRpc(localPlayerIndex);
			}
		}

		[Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
		private void DidSpawnPlayerClientRpc(NetworkObjectReference playerObjectRef, Int32 localPlayerIndex)
		{
			Debug.Log($"did spawn: {playerObjectRef.NetworkObjectId}");

			if (IsOwner)
			{
				var net = NetworkManager.Singleton;
				var player = net.SpawnManager.SpawnedObjects[playerObjectRef.NetworkObjectId];
				m_SpawnLocalPlayerTcs[localPlayerIndex].SetResult(player.GetComponent<LocalPlayer>());
				m_SpawnLocalPlayerTcs[localPlayerIndex] = null;
			}
		}

		[Rpc(SendTo.Owner, DeferLocal = true)]
		private void FailedSpawnPlayerClientRpc(Int32 localPlayerIndex)
		{
			Debug.Log($"FAILED spawn of player {localPlayerIndex}");

			m_SpawnLocalPlayerTcs[localPlayerIndex].SetResult(null);
			m_SpawnLocalPlayerTcs[localPlayerIndex] = null;
		}
	}
}
