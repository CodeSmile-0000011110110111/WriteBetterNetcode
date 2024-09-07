// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players.Couch
{
	[DisallowMultipleComponent]
	public sealed class CouchPlayersVars : NetworkBehaviour
	{
		// serialized only for debugging
		[SerializeField] private NetworkList<NetworkObjectReference> m_RemotePlayerReferences;

		private CouchPlayers m_CouchPlayers;

		private void Awake()
		{
			m_CouchPlayers = GetComponent<CouchPlayers>();

			var empty = new NetworkObjectReference[] { default, default, default, default };
			m_RemotePlayerReferences = new NetworkList<NetworkObjectReference>(empty);
		}

		internal void SetPlayerReference(Int32 playerIndex, NetworkObject playerObj) =>
			m_RemotePlayerReferences[playerIndex] = playerObj;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner == false)
				m_RemotePlayerReferences.OnListChanged += OnRemotePlayerReferencesChanged;
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			if (IsOwner == false)
				m_RemotePlayerReferences.OnListChanged -= OnRemotePlayerReferencesChanged;
		}

		private void OnRemotePlayerReferencesChanged(NetworkListEvent<NetworkObjectReference> changeEvent)
		{
			if (changeEvent.Type == NetworkListEvent<NetworkObjectReference>.EventType.Value)
			{
				var playerIndex = changeEvent.Index;

				Debug.Log($"Remote Player {playerIndex} changed with NetObjID: " + changeEvent.Value.NetworkObjectId);
				if (changeEvent.Value.TryGet(out var playerObj))
					m_CouchPlayers.RemotePlayerJoined(playerIndex, playerObj.GetComponent<Player>());
				else
					m_CouchPlayers.RemotePlayerLeft(playerIndex);
			}
		}
	}
}
