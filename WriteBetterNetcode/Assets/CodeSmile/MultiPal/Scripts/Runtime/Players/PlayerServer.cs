// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
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
		[Header("Test")]
		[SerializeField] private Boolean m_EnableTest_RandomlyKillPlayer;
		[SerializeField] [Range(1f, 60f)] private Single m_TestTimeToKill = 5f;
		[SerializeField] [Range(1f, 10f)] private Single m_TestTimeToRespawn = 2.5f;

		private PlayerClient m_ClientSide;

		private void Awake() => m_ClientSide = GetComponent<PlayerClient>();

		private void OnEnable() => StartCoroutine(TestRandomlyKillPlayer());

		private void OnDisable() => StopAllCoroutines();

		private IEnumerator TestRandomlyKillPlayer()
		{
			while (true)
			{
				var timeToDie = Random.value * m_TestTimeToKill + m_TestTimeToKill;
				yield return new WaitForSeconds(timeToDie);

				if (m_EnableTest_RandomlyKillPlayer)
					m_ClientSide.KillPlayerClientRpc();

				yield return new WaitForSeconds(m_TestTimeToRespawn);

				m_ClientSide.RespawnPlayerClientRpc();
			}
		}
	}
}
