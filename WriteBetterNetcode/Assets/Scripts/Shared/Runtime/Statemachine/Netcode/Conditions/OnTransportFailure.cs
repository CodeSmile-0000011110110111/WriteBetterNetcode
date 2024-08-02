// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	public sealed class OnTransportFailure : FSM.ICondition
	{
		private Boolean m_DidTransportFailureOccur;

		public void OnStart(FSM sm)
		{
			m_DidTransportFailureOccur = false;

			var net = NetworkManager.Singleton;
			net.OnTransportFailure += OnTransportFailureEvent;
		}

		public void OnStop(FSM sm)
		{
			var net = NetworkManager.Singleton;
			if (net)
				net.OnTransportFailure -= OnTransportFailureEvent;
		}

		public void OnExitState(FSM sm) => m_DidTransportFailureOccur = false;

		public Boolean IsSatisfied(FSM sm) => m_DidTransportFailureOccur;

		private void OnTransportFailureEvent() => m_DidTransportFailureOccur = false;
	}
}
