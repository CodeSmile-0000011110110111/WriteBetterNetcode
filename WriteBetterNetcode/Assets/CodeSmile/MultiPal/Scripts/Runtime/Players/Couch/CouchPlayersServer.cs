// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.Extensions.Netcode;
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

		private Boolean IsOffline => NetworkManagerExt.IsOffline;

		private void Awake()
		{
			if (m_PlayerPrefab == null)
				throw new MissingReferenceException(nameof(m_PlayerPrefab));

			m_ClientSide = GetComponent<CouchPlayersClient>();
			m_Vars = GetComponent<CouchPlayersVars>();
		}

		public override void OnDestroy() => base.OnDestroy();

		internal void SpawnPlayer(UInt64 ownerId, Byte playerIndex, Byte avatarIndex)
		{
			if (IsOffline)
				SpawnPlayerServerSide(ownerId, playerIndex, avatarIndex);
			else
				SpawnPlayerServerRpc(ownerId, playerIndex, avatarIndex);
		}

		private void SpawnPlayerServerSide(UInt64 ownerId, Byte playerIndex, Byte avatarIndex)
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
			if (IsOffline == false)
			{
				playerObj.SpawnWithOwnership(ownerId);
				m_Vars.SetPlayerReference(playerIndex, playerObj);
			}

			var player = playerObj.GetComponent<Player>();
			player.AvatarIndex = avatarIndex;

			m_ClientSide.DidSpawnPlayer(playerObj, playerIndex);
		}

		[Rpc(SendTo.Server, DeferLocal = true)]
		private void SpawnPlayerServerRpc(UInt64 ownerId, Byte playerIndex, Byte avatarIndex) =>
			SpawnPlayerServerSide(ownerId, playerIndex, avatarIndex);

		internal void DespawnPlayer(Byte playerIndex, NetworkObject playerObj)
		{
			if (IsOffline)
				DespawnPlayerServerSide(playerIndex, playerObj);
			else
				DespawnPlayerServerRpc(playerIndex, playerObj);
		}

		private void DespawnPlayerServerSide(Byte playerIndex, NetworkObject playerObj)
		{
			m_Vars.SetPlayerReference(playerIndex, null);

			if (IsOffline)
				Destroy(playerObj.gameObject);
			else
				playerObj.Despawn();
		}

		[Rpc(SendTo.Server, DeferLocal = true)]
		private void DespawnPlayerServerRpc(Byte playerIndex, NetworkObjectReference playerRef)
		{
			// this should not fail
			playerRef.TryGet(out var playerObj);
			DespawnPlayerServerSide(playerIndex, playerObj);
		}
	}
}
