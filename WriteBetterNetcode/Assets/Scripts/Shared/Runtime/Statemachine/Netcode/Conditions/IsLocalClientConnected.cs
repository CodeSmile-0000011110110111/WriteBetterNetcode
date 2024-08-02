// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	public sealed class IsLocalClientConnected : FSM.ICondition
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

		public Boolean IsSatisfied(FSM sm) => m_IsClientConnected;

		private void OnConnectionEvent(NetworkManager nm, ConnectionEventData connectionData)
		{
			if (connectionData.EventType == ConnectionEvent.ClientConnected)
				m_IsClientConnected = true;
			else if (connectionData.EventType == ConnectionEvent.ClientDisconnected)
				m_IsClientConnected = false;
		}
	}
}
