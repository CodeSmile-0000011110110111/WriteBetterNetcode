// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Core.Utility;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Core.Statemachine.Netcode
{
	[Serializable]
	public struct NetcodeConfig
	{
		public NetcodeRole Role;

		// Byte is deliberate! Relay limit is 100 connections
		public Byte MaxConnections;

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
