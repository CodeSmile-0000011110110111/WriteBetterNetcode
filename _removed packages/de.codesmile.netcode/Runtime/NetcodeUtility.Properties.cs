// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.Netcode
{
	public static partial class NetcodeUtility
	{
		/// <summary>
		///     Set to true before starting to use Unity's Relay service for connections.
		/// </summary>
		public static Boolean UseRelayService { get; set; }
		/// <summary>
		///     Clients must assign their join code before starting. Server/Host assign the join code after starting, which
		///     should be provided to the user in some way (eg shown in a GUI label or copied to clipboard).
		/// </summary>
		public static String RelayJoinCode { get; set; }
		/// <summary>
		///     Defaults to 'dtls'.
		/// </summary>
		public static String RelayConnectionType { get; set; }
		/// <summary>
		///     Maximum connections accepted by the relay service.
		/// </summary>
		/// <remarks>
		///     As of February 2024 the limit is 100 connections. See: https://docs.unity.com/ugs/manual/relay/manual/limitations
		/// </remarks>
		public static Int32 RelayMaxConnections { get; set; }

		// To support disabled domain reload, static fields have to be manually reset to default values
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticFields()
		{
			UseRelayService = false;
			RelayJoinCode = String.Empty;
			RelayConnectionType = "dtls";
			RelayMaxConnections = 4;
		}
	}
}
