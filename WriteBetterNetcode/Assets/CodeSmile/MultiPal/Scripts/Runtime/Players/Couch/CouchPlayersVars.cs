// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Extensions.Netcode;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players.Couch
{
	[DisallowMultipleComponent]
	public sealed class CouchPlayersVars : NetworkBehaviour
	{
		private NetworkList<NetworkObjectReference> m_RemotePlayerReferences;
		private CouchPlayers m_CouchPlayers;
		private Boolean IsOffline => NetworkManagerExt.IsOffline;

		private void Awake()
		{
			m_CouchPlayers = GetComponent<CouchPlayers>();

			var empty = new NetworkObjectReference[] { default, default, default, default };
			m_RemotePlayerReferences = new NetworkList<NetworkObjectReference>(empty);
		}

		internal void SetPlayerReference(Int32 playerIndex, NetworkObject playerObj)
		{
			if (IsOffline)
				return;

			m_RemotePlayerReferences[playerIndex] = playerObj;
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner == false)
			{
				// couch players spawns before individual remote players
				StartCoroutine(JoinExistingRemotePlayersAfterDelay());

				m_RemotePlayerReferences.OnListChanged += OnRemotePlayerReferencesChanged;
			}
		}

		private IEnumerator JoinExistingRemotePlayersAfterDelay()
		{
			yield return new WaitForEndOfFrame();

			JoinAllExistingRemotePlayers();
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			if (IsOwner == false)
			{
				EmulateRemotePlayersLeaveOnDespawn();
				m_RemotePlayerReferences.OnListChanged -= OnRemotePlayerReferencesChanged;
			}
		}

		private void JoinAllExistingRemotePlayers()
		{
			for (var playerIndex = 0; playerIndex < m_RemotePlayerReferences.Count; playerIndex++)
			{
				var playerReference = m_RemotePlayerReferences[playerIndex];
				if (playerReference.TryGet(out var playerObj))
					m_CouchPlayers.RemotePlayerJoined(playerIndex, playerObj.GetComponent<Player>());
			}
		}

		private void EmulateRemotePlayersLeaveOnDespawn()
		{
			for (var playerIndex = 0; playerIndex < m_RemotePlayerReferences.Count; playerIndex++)
			{
				var playerReference = m_RemotePlayerReferences[playerIndex];
				if (playerReference.TryGet(out var _))
					m_CouchPlayers.RemotePlayerLeft(playerIndex);
			}
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
