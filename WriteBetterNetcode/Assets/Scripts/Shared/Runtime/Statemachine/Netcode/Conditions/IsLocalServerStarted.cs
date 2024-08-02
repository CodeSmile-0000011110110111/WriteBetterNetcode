// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	public sealed class IsLocalServerStarted : FSM.ICondition
	{
		private Boolean m_IsServerStarted;

		public void OnStart(FSM sm)
		{
			Debug.LogWarning("IsServerStarted starting .. " + GetHashCode());
			m_IsServerStarted = false;

			var net = NetworkManager.Singleton;
			net.OnServerStarted += OnServerStartedEvent;
			net.OnServerStopped += OnServerStoppedEvent;
			net.OnTransportFailure += OnTransportFailureEvent;
		}

		public void OnStop(FSM sm)
		{
			var net = NetworkManager.Singleton;
			if (net)
			{
				net.OnServerStarted -= OnServerStartedEvent;
				net.OnServerStopped -= OnServerStoppedEvent;
				net.OnTransportFailure -= OnTransportFailureEvent;
			}
		}

		public Boolean IsSatisfied(FSM sm) => m_IsServerStarted;

		private void OnServerStartedEvent()
		{
			Debug.LogWarning("IsServerStarted OnServerStartedEvent .. " + GetHashCode());

			m_IsServerStarted = true;
		}

		private void OnServerStoppedEvent(Boolean isHost) => m_IsServerStarted = false;
		private void OnTransportFailureEvent() => m_IsServerStarted = false;
	}
}
