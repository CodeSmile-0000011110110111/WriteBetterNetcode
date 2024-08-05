// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	public class ApplyTransportConfig : FSM.IAction
	{
		private readonly FSM.StructVariable<TransportConfig> m_ConfigVar;

		private ApplyTransportConfig() {} // forbidden default ctor

		public ApplyTransportConfig(FSM.StructVariable<TransportConfig> configVar) => m_ConfigVar = configVar;

		public void OnStart(FSM sm) {}

		public void Execute(FSM sm)
		{
			var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
			var config = m_ConfigVar.Value;

			transport.UseEncryption = config.UseEncryption;
			transport.UseWebSockets = config.AllowWebConnections;

			var connData = config.ConnectionData;
			if (config.UseRelayService)
				throw new NotImplementedException();

			transport.SetConnectionData(connData.Address, connData.Port, connData.ServerListenAddress);
		}
	}
}
