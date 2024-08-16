// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Core.Extensions.Netcode;
using CodeSmile.Core.Utility;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Statemachine.Netcode
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

		public static TransportConfig FromNetworkManagerWithCmdArgOverrides()
		{
			var config = FromNetworkManager();
			config.Address = CmdArgs.GetString(nameof(Address), config.Address);
			config.Port = (UInt16)CmdArgs.GetInt(nameof(Port), config.Port);
			config.ServerListenAddress =
				CmdArgs.GetString(nameof(ServerListenAddress), config.ServerListenAddress);
			config.UseEncryption =
				CmdArgs.GetBool(nameof(UseEncryption), config.UseEncryption);
			config.UseWebSockets =
				CmdArgs.GetBool(nameof(UseWebSockets), config.UseWebSockets);
			return config;
		}

		public override String ToString() => $"{nameof(TransportConfig)}(" +
		                                     $"{nameof(Address)}={Address}, " +
		                                     $"{nameof(Port)}={Port}, " +
		                                     $"{nameof(ServerListenAddress)}={ServerListenAddress}, " +
		                                     $"{nameof(UseWebSockets)}={UseWebSockets}, " +
		                                     $"{nameof(UseEncryption)}={UseEncryption})";
	}
}
