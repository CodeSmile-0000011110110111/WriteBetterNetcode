// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Players.Couch;
using CodeSmile.MultiPal.Settings;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Stats
{
	[DisallowMultipleComponent]
	public sealed class PlayerStats : MonoBehaviour
	{
		private readonly GameObject[] m_PlayerStatsObjects = new GameObject[Constants.MaxCouchPlayers];

		private void Awake() => ComponentsRegistry.Set(this);

		private void Start()
		{
			CouchPlayers.OnLocalCouchPlayersSpawn += OnLocalCouchPlayersSpawn;
			CouchPlayers.OnLocalCouchPlayersDespawn += OnLocalCouchPlayersDespawn;
		}

		private void OnLocalCouchPlayersSpawn(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoining += OnCouchPlayerJoining;
			couchPlayers.OnCouchPlayerLeft += OnCouchPlayerLeft;
		}

		private void OnLocalCouchPlayersDespawn(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoining -= OnCouchPlayerJoining;
			couchPlayers.OnCouchPlayerLeft -= OnCouchPlayerLeft;
		}

		private void OnCouchPlayerJoining(CouchPlayers couchPlayers, Int32 playerIndex) => CreateStatsObject(playerIndex);
		private void OnCouchPlayerLeft(CouchPlayers couchPlayers, Int32 playerIndex) => DestroyStatsObject(playerIndex);

		private void CreateStatsObject(Int32 playerIndex)
		{
			var statsObj = new GameObject($"Player{playerIndex} Stats", typeof(HitpointsStat));
			statsObj.transform.parent = transform;
			m_PlayerStatsObjects[playerIndex] = statsObj;
		}

		private void DestroyStatsObject(Int32 playerIndex)
		{
			var statsObj = m_PlayerStatsObjects[playerIndex];
			if (statsObj != null)
			{
				m_PlayerStatsObjects[playerIndex] = null;
				Destroy(statsObj);
			}
		}
	}
}
