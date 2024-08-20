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
	/// <summary>
	///     Represents the group of players (1-4) playing on a single client.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CouchPlayersClient), typeof(CouchPlayersServer))]
	public sealed class CouchPlayers : NetworkBehaviour
	{
		internal const Int32 MaxCouchPlayers = 4;

		private readonly Player[] m_Players = new Player[MaxCouchPlayers];

		private CouchPlayersClient m_Client;

		public Player this[Int32 index] => m_Players[index];

		public static CouchPlayers Instance { get; private set; }

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

		private static void SetPlayerDebugName(Player player, Int32 couchPlayerIndex, String suffix = "") =>
			player.name = player.name.Replace("(Clone)", $" #{couchPlayerIndex}{suffix}");

		private void Awake() => m_Client = GetComponent<CouchPlayersClient>();

		public override async void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner)
			{
				Instance = this;

				Random.InitState(DateTime.Now.Millisecond);

				var posY = OwnerClientId * 2f;
				m_Players[0] = await m_Client.Spawn(0, 0);
				SetPlayerDebugName(m_Players[0], 0);
				m_Players[0].transform.position = new Vector3(-3, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[0].name}");

				m_Players[1] = await m_Client.Spawn(1, 1);
				SetPlayerDebugName(m_Players[1], 1);
				m_Players[1].transform.position = new Vector3(-1, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[1].name}");

				m_Players[2] = await m_Client.Spawn(2, 2);
				SetPlayerDebugName(m_Players[2], 2);
				m_Players[2].transform.position = new Vector3(1, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[2].name}");

				m_Players[3] = await m_Client.Spawn(3, 3);
				SetPlayerDebugName(m_Players[3], 3);
				m_Players[3].transform.position = new Vector3(3, posY, 0);
				Debug.Log($"{Time.frameCount} player spawned: {m_Players[3].name}");

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
				for (var i = 0; i < MaxCouchPlayers; i++)
				{
					yield return new WaitForSecondsRealtime(1.512473199f);

					if (m_Players[i] != null)
					{
						var avatarIndex = GetNewAvatarIndex(m_Players[i]);

						//Debug.Log($"set random avatar index {avatarIndex} to player {i}, client {OwnerClientId}");
						m_Players[i].AvatarIndex = (Byte)avatarIndex;
					}
				}
			} while (true);
		}

		public void RegisterRemotePlayer(Player player, Int32 couchPlayerIndex)
		{
			if (m_Players[couchPlayerIndex] != null)
				throw new Exception($"player {couchPlayerIndex} already exists");

			SetPlayerDebugName(player, couchPlayerIndex, " (Remote)");
			m_Players[couchPlayerIndex] = player;
		}
	}
}