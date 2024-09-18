// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	/// <summary>
	/// Tests if the client that this script executes on is currently connected to a network session.
	/// </summary>
	public class IsLocalClientConnected : ICondition
	{
		private Boolean m_IsClientConnected;

		public void OnStart(FSM sm)
		{
			m_IsClientConnected = false;

			var net = NetworkManager.Singleton;
			net.OnConnectionEvent += OnConnectionEvent;
		}

		public void OnStop(FSM sm)
		{
			var net = NetworkManager.Singleton;
			if (net)
				net.OnConnectionEvent -= OnConnectionEvent;
		}

		public virtual Boolean IsSatisfied(FSM sm) => m_IsClientConnected;

		private void OnConnectionEvent(NetworkManager nm, ConnectionEventData connectionData) => m_IsClientConnected =
			connectionData.EventType switch
			{
				ConnectionEvent.ClientConnected => true,
				ConnectionEvent.ClientDisconnected => false,
				_ => m_IsClientConnected,
			};
	}
}
