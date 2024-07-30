// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     On network spawn, enables components or game objects depending on ownership.
	/// </summary>
	[DisallowMultipleComponent]
	public class NetworkSpawnEnableComponents : NetworkSpawnTaskBehaviour
	{
		[Tooltip("Specify components to enable on spawn, in this order, if the object is locally owned. " +
		         "Adding a Transform component means all components on that GameObject will be enabled.")]
		[SerializeField] private Component[] m_EnableIfLocalOwner;

		[Tooltip("Specify components to enable on spawn, in this order, if the object is remotely owned. " +
		         "Adding a Transform component means all components on that GameObject will be enabled.")]
		[SerializeField] private Component[] m_EnableIfRemoteOwner;

		private void Start()
		{
			// always enable when not networked
			if (NetworkManager == null || NetworkManager.IsListening == false)
				EnableComponents(m_EnableIfLocalOwner);
		}

		public override void OnNetworkSpawn()
		{
			EnableComponents(IsOwner ? m_EnableIfLocalOwner : m_EnableIfRemoteOwner);
			base.OnNetworkSpawn();
		}

		private void EnableComponents(Component[] components)
		{
			if (components == null)
				return;

			foreach (var component in components)
			{
				if (component != null)
				{
					// Debug.Log($"NetworkSpawn enabling component: {component.GetType().Name}");

					if (component is Transform t)
					{
						// if a Transform was added it is interpreted as: enable all components
						t.gameObject.SetActive(true);
						foreach (MonoBehaviour mb in t.GetComponents(typeof(MonoBehaviour)))
							mb.enabled = true;
					}
					else if (component is MonoBehaviour mb)
						mb.enabled = true;
					else if (component is Collider cc)
						cc.enabled = true;
					else
						throw new ArgumentException($"unhandled type {component.GetType()}");
				}
			}
		}
	}
}
