// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Game
{
	[DisallowMultipleComponent]
	public sealed class ServerPlayerSpawner : NetworkBehaviour
	{
		private readonly List<LocalPlayer> m_AvatarPrefabs = new();

		private ClientPlayerSpawner m_ClientPlayerSpawner;

		private void Awake() => m_ClientPlayerSpawner = GetComponent<ClientPlayerSpawner>();

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsServer)
				FindSpawnablePlayers();
		}

		private void FindSpawnablePlayers()
		{
			var netPrefabs = NetworkManager.Singleton.NetworkConfig.Prefabs;
			foreach (var netPrefab in netPrefabs.Prefabs)
			{
				if (netPrefab.Prefab.TryGetComponent<LocalPlayer>(out var localPlayerPrefab))
					m_AvatarPrefabs.Add(localPlayerPrefab);
			}

			if (m_AvatarPrefabs.Count == 0)
				throw new NetworkConfigurationException("no LocalPlayer in NetworkPrefabs list!");
		}

		[Rpc(SendTo.Server, DeferLocal = true)]
		internal void SpawnPlayerServerRpc(UInt64 ownerId, Int32 localPlayerIndex, Int32 avatarIndex)
		{
			avatarIndex = Mathf.Clamp(avatarIndex, 0, m_AvatarPrefabs.Count - 1);

			var playerPrefab = m_AvatarPrefabs[avatarIndex];
			var playerNetObject = Instantiate(playerPrefab).GetComponent<NetworkObject>();
			playerNetObject.SpawnAsPlayerObject(ownerId);

			m_ClientPlayerSpawner.DidSpawnPlayerClientRpc(playerNetObject, localPlayerIndex);
		}
	}
}
