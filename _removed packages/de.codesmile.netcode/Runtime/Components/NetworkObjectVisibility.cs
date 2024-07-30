// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     Allows setting the network visibility of the object via Inspector.
	/// </summary>
	[RequireComponent(typeof(NetworkObject))]
	[DisallowMultipleComponent]
	public class NetworkObjectVisibility : NetworkBehaviour
	{
		[Tooltip("Network visibility of the object. If not visible, the object will not spawn.")]
		[SerializeField] private Visibility m_Visibility;

		private void Awake()
		{
			var netObject = GetComponent<NetworkObject>();
			if (netObject.CheckObjectVisibility != null)
				throw new InvalidOperationException("CheckObjectVisibility delegate already assigned");

			netObject.CheckObjectVisibility = GetVisibility();

			enabled = false;
		}

		private NetworkObject.VisibilityDelegate GetVisibility() => clientId =>
		{
			return m_Visibility switch
			{
				Visibility.AllClients => true,
				Visibility.NonOwnerClients => clientId != OwnerClientId,
				Visibility.OnlyOwnerClient => clientId == OwnerClientId,
				Visibility.NoClients => false,
				_ => throw new ArgumentOutOfRangeException(nameof(m_Visibility), m_Visibility.ToString()),
			};
		};

		private enum Visibility
		{
			AllClients,
			NoClients, // server only
			NonOwnerClients,
			OnlyOwnerClient,
		}
	}
}
