// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;

namespace CodeSmile.Netcode
{
	public class NetworkClientConfig : NetworkConfig
	{
		/// <summary>
		///     If provided, client will try to join a hosted match via the relay service's join code.
		///     <remarks>
		///         This code has to be transferred to the client via any means of external communication available
		///         to both host and client.
		///     </remarks>
		/// </summary>
		public String RelayJoinCode { get; set; } = String.Empty;

		/// <summary>
		///     Client will try to connect to the server using the given domain name or IPv4/IPv6 address.
		/// </summary>
		/// <example>
		///     <list type="">
		///         Valid entries are:
		///         - Domain names: <code>myserver.myhost.com</code>
		///         - IPv4 addresses: <code>174.175.176.177</code>
		///         - IPv6 addresses: <code>2001:0db8:85a3:0000:0000:8a2e:0370:7334</code>
		///     </list>
		/// </example>
		public String ServerAddress { get; set; } = "127.0.0.1";

		/// <summary>
		///     Client will try to connect to the server using the given port number.
		/// </summary>
		public UInt16 ServerPort { get; set; } = 7777;
	}
}
