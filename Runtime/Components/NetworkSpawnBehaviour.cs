// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     On network spawn performs a task depending on ownership.
	///     Currently allows setting the GameObject active or inactive, or set all child renderers to render only shadows.
	/// </summary>
	[DisallowMultipleComponent]
	public class NetworkSpawnBehaviour : NetworkBehaviour
	{
		[SerializeField] private SpawnTask m_LocalOwnerTask;
		[SerializeField] private SpawnTask m_RemoteOwnerTask;

		private void Start()
		{
			// if not networked, assume it's the local owner
			if (NetworkManager == null || NetworkManager.IsListening == false)
				PerformSpawnTask(m_LocalOwnerTask);
		}

		public override void OnNetworkSpawn()
		{
			PerformSpawnTask(IsOwner ? m_LocalOwnerTask : m_RemoteOwnerTask);
			enabled = false;
		}

		private void PerformSpawnTask(SpawnTask task)
		{
			switch (task)
			{
				case SpawnTask.DoNothing:
					break;
				case SpawnTask.SetActive:
					gameObject.SetActive(true);
					break;
				case SpawnTask.SetInactive:
					gameObject.SetActive(false);
					break;
				case SpawnTask.DrawOnlyShadows:
					DrawOnlyShadows();
					break;
				default:
					throw new ArgumentOutOfRangeException(task.ToString());
			}
		}

		private void DrawOnlyShadows()
		{
			foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
				meshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
		}

		private enum SpawnTask
		{
			DoNothing,
			SetActive,
			SetInactive,
			DrawOnlyShadows,
		}
	}
}
