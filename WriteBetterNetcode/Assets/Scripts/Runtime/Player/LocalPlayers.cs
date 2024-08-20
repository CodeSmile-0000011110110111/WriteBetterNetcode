// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerClientRpc))]
	public sealed class LocalPlayers : NetworkBehaviour
	{
		internal const Int32 MaxLocalPlayers = 4;

		private readonly LocalPlayer[] m_Players = new LocalPlayer[MaxLocalPlayers];

		private PlayerClientRpc m_Rpc;

		public static LocalPlayers Instance { get; private set; }

		private static Int32 GetNewAvatarIndex(LocalPlayer player)
		{
			var activeIndex = player.Avatar.ActiveIndex;
			var newIndex = 0;
			do
			{
				newIndex = Random.Range(0, 4);
			} while (activeIndex == newIndex);
			return newIndex;
		}

		private void Awake() => m_Rpc = GetComponent<PlayerClientRpc>();

		public override async void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner)
			{
				Instance = this;

				Random.seed = DateTime.Now.Millisecond;

				var posY = OwnerClientId * 2f;
				m_Players[0] = await m_Rpc.Spawn(0, 0);
				m_Players[0].transform.position = new Vector3(-3, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[0]}");

				m_Players[1] = await m_Rpc.Spawn(1, 1);
				m_Players[1].transform.position = new Vector3(-1, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[1]}");

				m_Players[2] = await m_Rpc.Spawn(2, 2);
				m_Players[2].transform.position = new Vector3(1, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[2]}");

				m_Players[3] = await m_Rpc.Spawn(3, 3);
				m_Players[3].transform.position = new Vector3(3, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[3]}");

				StartCoroutine(RandomizeAvatar());
			}
		}

		public override void OnNetworkDespawn()
		{
			StopAllCoroutines();

			if (IsOwner)
				Instance = null;
		}

		private IEnumerator RandomizeAvatar()
		{
			do
			{
				for (var i = 0; i < MaxLocalPlayers; i++)
				{
					yield return new WaitForSecondsRealtime(.612773199f);

					var newIndex = GetNewAvatarIndex(m_Players[i]);
					m_Rpc.SetAvatar(m_Players[i].NetworkObject, newIndex);
				}
			} while (true);
		}
	}
}
