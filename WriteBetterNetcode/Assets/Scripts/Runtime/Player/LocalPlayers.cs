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
	[RequireComponent(typeof(LocalPlayersClientRpc), typeof(LocalPlayersServerRpc))]
	public sealed class LocalPlayers : NetworkBehaviour
	{
		internal const Int32 MaxLocalPlayers = 4;

		private readonly Player[] m_Players = new Player[MaxLocalPlayers];

		private LocalPlayersClientRpc m_ClientRpc;

		public Player this[Int32 index] => m_Players[index];

		public static LocalPlayers Instance { get; private set; }

		private static Int32 GetNewAvatarIndex(Player playerAvatar)
		{
			var curAvatarIndex = playerAvatar.AvatarIndex;
			var newAvatarIndex = 0;

			// poor dev's shuffle
			do
			{
				newAvatarIndex = Random.Range(0, 4);
			} while (curAvatarIndex == newAvatarIndex);

			return newAvatarIndex;
		}

		private void Awake() => m_ClientRpc = GetComponent<LocalPlayersClientRpc>();

		public override async void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner)
			{
				Instance = this;

				Random.InitState(DateTime.Now.Millisecond);

				var posY = OwnerClientId * 2f;
				m_Players[0] = await m_ClientRpc.Spawn(0, 0);
				m_Players[0].name = m_Players[0].name.Replace("(Clone)", " #0");
				m_Players[0].transform.position = new Vector3(-3, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[0]}");

				m_Players[1] = await m_ClientRpc.Spawn(1, 1);
				m_Players[1].name = m_Players[1].name.Replace("(Clone)", " #1");
				m_Players[1].transform.position = new Vector3(-1, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[1]}");

				m_Players[2] = await m_ClientRpc.Spawn(2, 2);
				m_Players[2].name = m_Players[2].name.Replace("(Clone)", " #2");
				m_Players[2].transform.position = new Vector3(1, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[2]}");

				m_Players[3] = await m_ClientRpc.Spawn(3, 3);
				m_Players[3].name = m_Players[3].name.Replace("(Clone)", " #3");
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
			yield return new WaitForSecondsRealtime(1f);

			do
			{
				for (var i = 0; i < MaxLocalPlayers; i++)
				{
					yield return new WaitForSecondsRealtime(.612773199f);

					if (m_Players[i] != null)
					{
						var avatarIndex = GetNewAvatarIndex(m_Players[i]);
						m_Players[i].AvatarIndex = avatarIndex;
					}
				}
			} while (true);
		}
	}
}
