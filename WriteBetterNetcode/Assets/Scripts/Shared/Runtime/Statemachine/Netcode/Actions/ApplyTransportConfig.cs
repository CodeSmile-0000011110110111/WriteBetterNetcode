// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	public class ApplyTransportConfig : IAction
	{
		private readonly StructVar<TransportConfig> m_TransportConfigVar;
		private readonly StructVar<RelayConfig> m_RelayConfigVar;

		private ApplyTransportConfig() {} // forbidden default ctor

		public ApplyTransportConfig(StructVar<TransportConfig> transportConfigVar) => m_TransportConfigVar = transportConfigVar;

		public void OnStart(FSM sm) {}

		public void Execute(FSM sm)
		{
			var transportConfig = m_TransportConfigVar.Value;

			var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
			if (transportConfig.UseEncryption)
			{
				// TODO: set server/client secrets
				throw new NotImplementedException();
			}

			transport.UseEncryption = transportConfig.UseEncryption;
			transport.UseWebSockets = transportConfig.UseWebSockets;
			transport.SetConnectionData(transportConfig.Address, transportConfig.Port, transportConfig.ServerListenAddress);
		}
	}
}
