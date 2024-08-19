// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(LocalPlayerSpawner))]
	public sealed class LocalPlayers : NetworkBehaviour
	{
		internal const Int32 MaxLocalPlayers = 4;

		private readonly LocalPlayer[] m_Players = new LocalPlayer[MaxLocalPlayers];

		private LocalPlayerSpawner m_Spawner;

		private void Awake()
		{
			m_Spawner = GetComponent<LocalPlayerSpawner>();
			InitPlayers();
		}

		public override async void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			Debug.Log($"{Time.frameCount} request player spawn ...");

			m_Players[0] = await m_Spawner.SpawnWithLocalPlayerIndex(0);
			Debug.Log($"{Time.frameCount} player spawned: {m_Players[0]}");
			m_Players[1] = await m_Spawner.SpawnWithLocalPlayerIndex(1);
			Debug.Log($"{Time.frameCount} player spawned: {m_Players[1]}");
			m_Players[2] = await m_Spawner.SpawnWithLocalPlayerIndex(2);
			Debug.Log($"{Time.frameCount} player spawned: {m_Players[2]}");
			m_Players[3] = await m_Spawner.SpawnWithLocalPlayerIndex(3);
			Debug.Log($"{Time.frameCount} player spawned: {m_Players[3]}");

			if (m_Players[0] != null)
				m_Players[0].transform.position = new Vector3(-3, 0, 0);
			if (m_Players[1] != null)
				m_Players[1].transform.position = new Vector3(-1, 0, 0);
			if (m_Players[2] != null)
				m_Players[2].transform.position = new Vector3(1, 0, 0);
			if (m_Players[3] != null)
				m_Players[3].transform.position = new Vector3(3, 0, 0);
		}

		private void InitPlayers()
		{
			for (var i = 0; i < MaxLocalPlayers; i++)
				m_Players[i] = null;
		}
	}
}
