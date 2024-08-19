// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Game
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ClientPlayerSpawner))]
	public sealed class LocalPlayers : NetworkBehaviour
	{
		internal const Int32 MaxLocalPlayers = 4;

		private readonly LocalPlayer[] m_Players = new LocalPlayer[MaxLocalPlayers];

		private ClientPlayerSpawner m_Spawner;

		public static LocalPlayers Instance { get; private set; }

		private void Awake() => m_Spawner = GetComponent<ClientPlayerSpawner>();

		public override async void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner)
			{
				Instance = this;

				var posY = OwnerClientId * 2f;
				m_Players[0] = await m_Spawner.Spawn(0, 0);
				m_Players[0].transform.position = new Vector3(-3, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[0]}");

				m_Players[1] = await m_Spawner.Spawn(1, 1);
				m_Players[1].transform.position = new Vector3(-1, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[1]}");

				m_Players[2] = await m_Spawner.Spawn(2, 2);
				m_Players[2].transform.position = new Vector3(1, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[2]}");

				m_Players[3] = await m_Spawner.Spawn(3, 3);
				m_Players[3].transform.position = new Vector3(3, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[3]}");
			}
		}

		public override void OnNetworkDespawn()
		{
			if (IsOwner)
				Instance = null;
		}
	}
}
