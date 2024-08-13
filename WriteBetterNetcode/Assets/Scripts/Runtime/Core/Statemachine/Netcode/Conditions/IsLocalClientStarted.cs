// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Statemachine.Netcode.Conditions
{
	public class IsLocalClientStarted : ICondition
	{
		private Boolean m_IsClientStarted;

		public void OnStart(FSM sm)
		{
			m_IsClientStarted = false;

			var net = NetworkManager.Singleton;
			net.OnClientStarted += OnClientStartedEvent;
			net.OnClientStopped += OnClientStoppedEvent;
			net.OnTransportFailure += OnTransportFailureEvent;
		}

		public void OnStop(FSM sm)
		{
			var net = NetworkManager.Singleton;
			if (net)
			{
				net.OnClientStarted -= OnClientStartedEvent;
				net.OnClientStopped -= OnClientStoppedEvent;
				net.OnTransportFailure -= OnTransportFailureEvent;
			}
		}

		public virtual Boolean IsSatisfied(FSM sm) => m_IsClientStarted;

		private void OnClientStartedEvent() => m_IsClientStarted = true;
		private void OnClientStoppedEvent(Boolean isHost) => m_IsClientStarted = false;
		private void OnTransportFailureEvent() => m_IsClientStarted = false;
	}
}
