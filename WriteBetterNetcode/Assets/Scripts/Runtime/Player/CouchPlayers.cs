// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using System.Threading.Tasks;
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
	[RequireComponent(typeof(CouchPlayersClient),
		typeof(CouchPlayersServer))]
	public sealed class CouchPlayers : NetworkBehaviour
	{
		internal const Int32 MaxCouchPlayers = 4;
		private readonly Player[] m_Players = new Player[MaxCouchPlayers];

		public Player this[Int32 index] =>
			index >= 0 && index < MaxCouchPlayers ? m_Players[index] : null;

		private CouchPlayersClient m_ClientSide;

		private static Int32 Test_ShuffleAvatarIndex(Player playerAvatar)
		{
			var curAvatarIndex = playerAvatar.AvatarIndex;
			var newAvatarIndex = 0;

			// poor dev's shuffle
			do
			{
				newAvatarIndex = Random.Range(0, 5);
			} while (curAvatarIndex == newAvatarIndex);

			return newAvatarIndex;
		}

		private static void SetPlayerDebugName(Player player, Int32 couchPlayerIndex, String suffix = "") =>
			player.name = player.name.Replace("(Clone)", $" #{couchPlayerIndex}{suffix}");

		private void Awake() => m_ClientSide = GetComponent<CouchPlayersClient>();

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner)
			{
				Test_SpawnPlayers();
				StartCoroutine(Test_ShuffleAvatar());
			}
		}

		private async void Test_SpawnPlayers()
		{
			Random.InitState(DateTime.Now.Millisecond);

			var posY = OwnerClientId * 2f;
			m_Players[0] = await m_ClientSide.Spawn(0, 0);
			SetPlayerDebugName(m_Players[0], 0);
			m_Players[0].transform.position = new Vector3(-3, posY, 0);

			m_Players[1] = await m_ClientSide.Spawn(1, 1);
			SetPlayerDebugName(m_Players[1], 1);
			m_Players[1].transform.position = new Vector3(-1, posY, 0);

			m_Players[2] = await m_ClientSide.Spawn(2, 2);
			SetPlayerDebugName(m_Players[2], 2);
			m_Players[2].transform.position = new Vector3(1, posY, 0);

			m_Players[3] = await m_ClientSide.Spawn(3, 3);
			SetPlayerDebugName(m_Players[3], 3);
			m_Players[3].transform.position = new Vector3(3, posY, 0);
		}

		public override void OnNetworkDespawn() => StopAllCoroutines();

		private IEnumerator Test_ShuffleAvatar()
		{
			do
			{
				for (var i = 0; i < MaxCouchPlayers; i++)
				{
					yield return new WaitForSecondsRealtime(1.132473199f);

					if (m_Players[i] != null)
					{
						var avatarIndex = Test_ShuffleAvatarIndex(m_Players[i]);
						m_Players[i].AvatarIndex = (Byte)avatarIndex;
					}
				}
			} while (true);
		}

		public void AddRemotePlayer(Player player, Int32 couchPlayerIndex)
		{
			if (m_Players[couchPlayerIndex] != null)
				throw new Exception($"player {couchPlayerIndex} already exists");

			SetPlayerDebugName(player, couchPlayerIndex, " (Remote)");
			m_Players[couchPlayerIndex] = player;
		}
	}
}
