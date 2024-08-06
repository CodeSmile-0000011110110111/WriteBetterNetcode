﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Services;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	public class TransportSetup : IAction
	{
		private readonly Var<NetcodeConfig> m_NetcodeConfigVar;
		private readonly Var<TransportConfig> m_TransportConfigVar;
		private readonly Var<RelayConfig> m_RelayConfigVar;

		private TransportSetup() {} // forbidden default ctor

		public TransportSetup(Var<NetcodeConfig> netcodeConfigVar,
			Var<TransportConfig> transportConfigVar,
			Var<RelayConfig> relayConfigVar)
		{
			m_NetcodeConfigVar = netcodeConfigVar;
			m_TransportConfigVar = transportConfigVar;
			m_RelayConfigVar = relayConfigVar;
		}

		public void Execute(FSM sm)
		{
			var netcodeConfig = m_NetcodeConfigVar.Value;
			var transportConfig = m_TransportConfigVar.Value;
			var relayConfig = m_RelayConfigVar.Value;

			var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
			transport.UseEncryption = transportConfig.UseEncryption;
			transport.UseWebSockets = transportConfig.UseWebSockets;

			var connectionType = transportConfig.UseEncryption ? transportConfig.UseWebSockets ? "wss" : "dtls" : "udp";

			if (transportConfig.UseEncryption)
				throw new NotImplementedException("encryption ...");

			if (relayConfig.UseRelayService)
				transport.SetRelayServerData(new RelayServerData(relayConfig.HostAllocation, connectionType));
			else
				transport.SetConnectionData(transportConfig.Address, transportConfig.Port, transportConfig.ServerListenAddress);
		}
	}
}