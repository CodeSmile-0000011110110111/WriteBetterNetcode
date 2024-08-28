// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Players;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GUI
{
	[DisallowMultipleComponent]
	public sealed class GuiController : MonoBehaviour
	{
		private CouchPlayers m_CouchPlayers;

		private void Awake()
		{
			CouchPlayers.OnCouchSessionStarted += OnCouchSessionStarted;
			CouchPlayers.OnCouchSessionStopped += OnCouchSessionStopped;
		}

		private void OnDestroy()
		{
			CouchPlayers.OnCouchSessionStarted -= OnCouchSessionStarted;
			CouchPlayers.OnCouchSessionStopped -= OnCouchSessionStopped;
		}

		private void OnCouchSessionStarted(CouchPlayers localCouchPlayers)
		{
			m_CouchPlayers = localCouchPlayers;
			m_CouchPlayers.OnCouchPlayerJoin += OnCouchPlayerJoin;
			m_CouchPlayers.OnCouchPlayerLeave += OnCouchPlayerLeave;
		}

		private void OnCouchSessionStopped() => m_CouchPlayers = null;

		private void OnCouchPlayerJoin(Int32 playerIndex)
		{
			var player = m_CouchPlayers[playerIndex];
			player.OnRequestPause += OnPlayerRequestPause;
		}

		private void OnCouchPlayerLeave(Int32 playerIndex) => Debug.Log($"LEAVE {playerIndex}");

		private void OnPlayerRequestPause(Player player)
		{
			Debug.Log("pause requested");
		}
	}
}
