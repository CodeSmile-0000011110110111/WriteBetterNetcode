// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     On network spawn, performs one-time initialization tasks like disable component or deactivate GameObject.
	/// </summary>
	public class NetworkSpawnTaskBehaviour : NetworkBehaviour
	{
		[SerializeField] private OnTaskPerformed m_OnTaskPerformed = OnTaskPerformed.DoNothing;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			TaskPerformed();
		}

		/// <summary>
		///     Call base.OnNetworkSpawn() or call this directly to flag the task as performed and to apply
		///     the OnTaskPerformed setting (disable/deactivate).
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		protected void TaskPerformed()
		{
			switch (m_OnTaskPerformed)
			{
				case OnTaskPerformed.DoNothing:
					break;
				case OnTaskPerformed.DisableComponent:
					enabled = false;
					break;
				case OnTaskPerformed.DeactivateGameObject:
					gameObject.SetActive(false);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(m_OnTaskPerformed));
			}
		}

		private enum OnTaskPerformed
		{
			DoNothing,
			DisableComponent,
			DeactivateGameObject,
		}
	}
}
