// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Extensions.Netcode;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	[Serializable]
	public struct TransportConfig
	{
		public String Address;
		public UInt16 Port;
		public String ServerListenAddress;
		public Boolean UseEncryption;
		public Boolean UseWebSockets;

		public static TransportConfig FromNetworkManager()
		{
			var transport = NetworkManager.Singleton.GetTransport();
			var connData = transport.ConnectionData;

			return new TransportConfig
			{
				Address = connData.Address,
				Port = connData.Port,
				ServerListenAddress = connData.ServerListenAddress,

				UseEncryption = transport.UseEncryption,
				UseWebSockets = transport.UseWebSockets,
			};
		}

		public override String ToString() =>
			$"{nameof(TransportConfig)}(Address={Address}:{Port}, ListenAddress={ServerListenAddress}, " +
			$"UseWebSockets={UseWebSockets}, UseEncryption={UseEncryption})";
	}
}
