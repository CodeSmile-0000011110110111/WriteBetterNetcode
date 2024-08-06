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
		private readonly FSM.StructVar<TransportConfig> m_ConfigVar;

		private ApplyTransportConfig() {} // forbidden default ctor

		public ApplyTransportConfig(FSM.StructVar<TransportConfig> configVar) => m_ConfigVar = configVar;

		public void OnStart(FSM sm) {}

		public void Execute(FSM sm)
		{
			var config = m_ConfigVar.Value;

			var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
			if (config.UseEncryption)
			{
				// TODO: set server/client secrets
				throw new NotImplementedException();
			}

			transport.UseEncryption = config.UseEncryption;
			transport.UseWebSockets = config.UseWebSockets;
			transport.SetConnectionData(config.Address, config.Port, config.ServerListenAddress);
		}
	}
}
