// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	public class IsServerOnline : FSM.ICondition
	{
		private Boolean m_IsOnline;

		public void OnStart(FSM sm)
		{
			var net = NetworkManager.Singleton;
			net.OnServerStarted += OnServerStarted;
			net.OnServerStopped += OnServerStopped;
			net.OnTransportFailure += OnTransportFailure;
		}

		public void OnStop(FSM sm)
		{
			var net = NetworkManager.Singleton;
			if (net)
			{
				net.OnServerStarted -= OnServerStarted;
				net.OnServerStopped -= OnServerStopped;
				net.OnTransportFailure -= OnTransportFailure;
			}
		}

		public Boolean IsSatisfied(FSM sm) => m_IsOnline;

		private void OnServerStarted() => m_IsOnline = true;
		private void OnServerStopped(Boolean isHost) => m_IsOnline = false;
		private void OnTransportFailure() => m_IsOnline = false;
	}
}
