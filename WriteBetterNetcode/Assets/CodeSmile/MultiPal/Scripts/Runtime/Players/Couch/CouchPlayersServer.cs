// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Design;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players.Couch
{
	[DisallowMultipleComponent]
	internal sealed class CouchPlayersServer : NetworkBehaviour
	{
		[SerializeField] private NetworkObject m_PlayerPrefab;

		private CouchPlayersClient m_ClientSide;
		private CouchPlayersVars m_Vars;

		private void Awake()
		{
			if (m_PlayerPrefab == null)
				throw new MissingReferenceException(nameof(m_PlayerPrefab));

			m_ClientSide = GetComponent<CouchPlayersClient>();
			m_Vars = GetComponent<CouchPlayersVars>();
			SpawnLocations.OnSpawnLocationsChanged += OnSpawnLocationsChanged;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			SpawnLocations.OnSpawnLocationsChanged -= OnSpawnLocationsChanged;
		}

		private void OnSpawnLocationsChanged(SpawnLocations spawnLocations)
		{
			if (spawnLocations.Count > 0)
				m_ClientSide.ServerCanSpawnPlayersClientRpc();
		}

		[Rpc(SendTo.Server, DeferLocal = true)]
		internal void SpawnPlayerServerRpc(UInt64 ownerId, Byte playerIndex, Byte avatarIndex)
		{
			var position = Vector3.zero;
			var rotation = Quaternion.identity;
			var spawnLocations = ComponentsRegistry.Get<SpawnLocations>();
			var location = spawnLocations.GetRandomSpawnLocation(playerIndex);
			if (location != null)
			{
				var t = location.transform;
				position = t.position;
				rotation = t.rotation;
			}

			var playerGo = Instantiate(m_PlayerPrefab, position, rotation);
			var playerObj = playerGo.GetComponent<NetworkObject>();
			playerObj.SpawnWithOwnership(ownerId);

			m_Vars.SetPlayerReference(playerIndex, playerObj);

			var player = playerObj.GetComponent<Player>();
			player.AvatarIndex = avatarIndex;

			m_ClientSide.DidSpawnPlayerClientRpc(playerObj, playerIndex);
		}

		[Rpc(SendTo.Server, DeferLocal = true)]
		public void DespawnPlayerServerRpc(Byte playerIndex, NetworkObjectReference playerRef)
		{
			if (playerRef.TryGet(out var playerObj))
			{
				m_Vars.SetPlayerReference(playerIndex, null);
				playerObj.Despawn();
			}
			else
				Debug.LogWarning($"could not despawn {playerRef.NetworkObjectId}");
		}
	}
}
