// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Statemachine.Netcode.Actions
{
	public sealed class NetworkStart : IAction
	{
		private readonly Var<NetcodeConfig> m_NetcodeConfigVar;

		public NetworkStart(Var<NetcodeConfig> netcodeConfigVar) => m_NetcodeConfigVar = netcodeConfigVar;

		public void Execute(FSM sm)
		{
			var succeeded = false;
			var net = NetworkManager.Singleton;
			var role = m_NetcodeConfigVar.Value.Role;

			switch (role)
			{
				case NetcodeRole.Client:
					succeeded = net.StartClient();
					break;
				case NetcodeRole.Host:
					succeeded = net.StartHost();
					break;
				case NetcodeRole.Server:
					succeeded = net.StartServer();
					break;

				case NetcodeRole.None:
				default:
					throw new ArgumentOutOfRangeException(nameof(role));
			}

			if (succeeded == false)
				throw new Exception($"NetworkManager.Start{role} failed");
		}
	}
}
