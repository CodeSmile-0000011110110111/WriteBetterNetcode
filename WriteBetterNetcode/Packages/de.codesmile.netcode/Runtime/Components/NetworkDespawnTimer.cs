// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     (Server only) On spawn, starts a coroutine that waits until the time elapsed, then despawns the object.
	/// </summary>
	public class NetworkDespawnTimer : NetworkBehaviour
	{
		/// <summary>
		///     Event is invoked when the timer elapsed.
		/// </summary>
		public event Action OnTimerElapsed;

		[SerializeField] private Single m_SecondsTillDespawn = 3f;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsServer)
				StartCoroutine(DespawnWhenTimeOut());
		}

		private IEnumerator DespawnWhenTimeOut()
		{
			yield return new WaitForSeconds(m_SecondsTillDespawn);

			OnTimerElapsed?.Invoke();

			GetComponent<NetworkObject>().Despawn();
		}
	}
}
