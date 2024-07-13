// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;

namespace CodeSmile.Netcode
{
	public class NetworkServerConfig
	{
		/// <summary>
		///     Set to true before starting to use Unity's Relay service for client connections.
		///     <remarks>
		///         Relay is primarily meant for making client-hosted matches available online, rather than being
		///         restricted to the local network - unless the host is proficient in port forwarding and clients
		///         comfortable using IP addresses or dynamic DNS.
		///         A dedicated server deployed on a hosting service does not require the relay service and should not use it.
		///     </remarks>
		/// </summary>
		public Boolean UseRelayService { get; set; } = false;

		/// <summary>
		///     If relay is enabled and Server/Host successfully started a session, the session's join code is assigned here.
		///     <remarks>
		///         Display this code to the host. The host has to forward this code to users that wish to join.
		///     </remarks>
		/// </summary>
		public String RelayJoinCode { get; set; } = String.Empty;

		/// <summary>
		///     Defaults to 'dtls'.
		/// </summary>
		public String RelayConnectionType { get; set; } = "dtls";

		/// <summary>
		///     Maximum connections accepted by the relay service.
		/// </summary>
		/// <remarks>
		///     As of February 2024 the limit is 100 connections. See: https://docs.unity.com/ugs/manual/relay/manual/limitations
		/// </remarks>
		public Int32 RelayMaxConnections { get; set; } = 4;
	}
}
