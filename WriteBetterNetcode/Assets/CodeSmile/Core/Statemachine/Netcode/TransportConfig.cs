// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Extensions.Netcode;
using CodeSmile.Utility;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	/// <summary>
	/// DTO that provides Transport configuration options.
	/// </summary>
	[Serializable]
	public struct TransportConfig
	{
		/// <summary>
		/// The IP address (v4 or v6) to connect to.
		/// </summary>
		public String Address;
		/// <summary>
		/// The port number for the connection.
		/// </summary>
		public UInt16 Port;
		/// <summary>
		/// The ServerListenAddress. Should be 0.0.0.0 for publicly accessible games.
		/// Use 127.0.0.1 to allow only instances running on the same machine to join. (Default)
		/// </summary>
		public String ServerListenAddress;
		/// <summary>
		/// Enables transport-level encryption.
		/// </summary>
		public Boolean UseEncryption;
		/// <summary>
		/// Enables WebSocket support. Must be enabled for web Clients and the server that web Clients connect to.
		/// </summary>
		public Boolean UseWebSockets;

		/// <summary>
		/// Creates a TransportConfig instance from current NetworkManager Transport settings.
		/// Use this to get the Transport values set in the Inspector.
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Creates a TransportConfig instance from current NetworkManager Transport settings and then have command line
		/// arguments (if specified) override each setting.
		/// </summary>
		/// <returns></returns>
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
