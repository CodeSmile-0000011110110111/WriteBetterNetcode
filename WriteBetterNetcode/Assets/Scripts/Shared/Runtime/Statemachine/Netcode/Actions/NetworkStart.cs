// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	public sealed class NetworkStart : IAction
	{
		private readonly Var<NetcodeConfig> m_NetcodeConfigVar;

		public NetworkStart(Var<NetcodeConfig> netcodeConfigVar) => m_NetcodeConfigVar = netcodeConfigVar;

		public void Execute(FSM sm)
		{
			var role = m_NetcodeConfigVar.Value.Role;
			switch (role)
			{
				case NetcodeRole.Client:
					NetworkManager.Singleton.StartClient();
					break;
				case NetcodeRole.Host:
					NetworkManager.Singleton.StartHost();
					break;
				case NetcodeRole.Server:
					NetworkManager.Singleton.StartServer();
					break;

				case NetcodeRole.None:
				default:
					throw new ArgumentOutOfRangeException(nameof(role), role.ToString());
			}
		}
	}
}
