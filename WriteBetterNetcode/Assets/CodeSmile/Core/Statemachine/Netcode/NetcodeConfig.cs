// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Utility;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	/// <summary>
	///     DTO that provides configuration options for Transport setup and NetworkManager.
	/// </summary>
	[Serializable]
	public struct NetcodeConfig
	{
		/// <summary>
		///     The Role we want to play as.
		/// </summary>
		public NetcodeRole Role;

		/// <summary>
		///     Limits the maximum number of allowed connections.
		/// </summary>
		/// <remarks>
		///     Byte is deliberate, since Relay limit is 100 connections and a Netcode project with >250 clients is outside
		///     its intended scope.
		/// </remarks>
		public Byte MaxConnections;

		/// <summary>
		/// Create a NetcodeConfig from command line parameters. Omitted parameters will use default values.
		/// </summary>
		/// <returns></returns>
		public static NetcodeConfig FromCmdArgs() => new()
		{
			Role = GetRoleFromCmdArgs(),
			MaxConnections = (Byte)CmdArgs.GetInt(nameof(MaxConnections)),
		};

		private static NetcodeRole GetRoleFromCmdArgs()
		{
			var role = NetcodeRole.None;

			if (CmdArgs.Exists(NetcodeRole.Server.ToString()))
				role = NetcodeRole.Server;
			else if (CmdArgs.Exists(NetcodeRole.Host.ToString()))
				role = NetcodeRole.Host;
			else if (CmdArgs.Exists(NetcodeRole.Client.ToString()))
				role = NetcodeRole.Client;

			return role;
		}

		public override String ToString() => $"{nameof(NetcodeConfig)}(" +
		                                     $"{nameof(Role)}={Role}, " +
		                                     $"{nameof(MaxConnections)}={MaxConnections})";
	}
}
