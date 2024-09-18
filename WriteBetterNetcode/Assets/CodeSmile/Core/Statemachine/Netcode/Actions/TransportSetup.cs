// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	/// <summary>
	/// Configures NetworkManager's Transport from config variables.
	/// </summary>
	/// <remarks>Supports all aspects of Transport configuration, eg with or without Relay, WebSockets, etc.</remarks>
	public sealed class TransportSetup : IAction
	{
		private readonly Var<NetcodeConfig> m_NetcodeConfigVar;
		private readonly Var<TransportConfig> m_TransportConfigVar;
		private readonly Var<RelayConfig> m_RelayConfigVar;

		private TransportSetup() {} // forbidden default ctor

		/// <summary>
		/// Creates a new TransportSetup action.
		/// </summary>
		/// <param name="netcodeConfigVar"></param>
		/// <param name="transportConfigVar"></param>
		/// <param name="relayConfigVar"></param>
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
			var isWeb = Application.platform == RuntimePlatform.WebGLPlayer;

			var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
			transport.UseEncryption = transportConfig.UseEncryption;
			transport.UseWebSockets = transportConfig.UseWebSockets || isWeb;

			// TODO: transport encryption ...
			if (transportConfig.UseEncryption)
				throw new NotImplementedException("TODO: encryption ... set secrets etc");

			if (relayConfig.UseRelay)
			{
				var connectionType = transport.UseWebSockets ? "wss" : transport.UseEncryption ? "dtls" : "udp";
				transport.SetRelayServerData(netcodeConfig.Role == NetcodeRole.Client
					? new RelayServerData(relayConfig.JoinAllocation, connectionType)
					: new RelayServerData(relayConfig.HostAllocation, connectionType));
			}
			else
				transport.SetConnectionData(transportConfig.Address, transportConfig.Port, transportConfig.ServerListenAddress);
		}
	}
}
