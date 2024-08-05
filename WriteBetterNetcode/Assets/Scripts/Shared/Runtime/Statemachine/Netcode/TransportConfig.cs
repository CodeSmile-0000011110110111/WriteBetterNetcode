// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	[Serializable]
	public struct TransportConfig
	{
		public NetworkRole Role;
		public UnityTransport.ConnectionAddressData ConnectionData;
		public Boolean AllowWebConnections;
		public Boolean UseEncryption;
		public Boolean UseRelayService;
		public String RelayJoinCode;

		public override String ToString() =>
			$"{nameof(TransportConfig)}(Role={Role}, Address={ConnectionData.Address}:{ConnectionData.Port}, " +
			$"Listen={ConnectionData.ServerListenAddress}, WebSockets={AllowWebConnections}, " +
			$"Encryption={UseEncryption}, Relay={UseRelayService}, JoinCode={RelayJoinCode})";
	}
}
