// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeSmile.MultiPal.Players
{
	[DisallowMultipleComponent]
	internal class PlayerServer : NetworkBehaviour
	{
		private PlayerClient m_ClientSide;
		private Player m_Player;

		private void Awake()
		{
			m_ClientSide = GetComponent<PlayerClient>();
			m_Player = GetComponent<Player>();
		}

		private void OnEnable() => StartCoroutine(TestRandomlyKillPlayer());

		private void OnDisable() => StopAllCoroutines();

		private IEnumerator TestRandomlyKillPlayer()
		{
			while (true)
			{
				var timeToDie = Random.value * 5f + 5f;
				yield return new WaitForSeconds(timeToDie);

				m_ClientSide.KillPlayerClientRpc();

				var timeToRespawn = 3f;
				yield return new WaitForSeconds(timeToRespawn);

				m_ClientSide.RespawnPlayerClientRpc();
			}
		}
	}
}
