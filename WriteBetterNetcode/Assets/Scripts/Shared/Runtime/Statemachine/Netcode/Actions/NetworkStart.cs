// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Netcode.Enums;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	public sealed class NetworkStart : FSM.IAction
	{
		private readonly NetworkRole m_Role;

		public NetworkStart(NetworkRole role) => m_Role = role;

		public void Execute(FSM sm)
		{
			switch (m_Role)
			{
				case NetworkRole.Client:
					NetworkManager.Singleton.StartClient();
					break;
				case NetworkRole.Host:
					NetworkManager.Singleton.StartHost();
					break;
				case NetworkRole.Server:
					NetworkManager.Singleton.StartServer();
					break;

				case NetworkRole.None:
					throw new ArgumentException($"Can't start network with role '{m_Role}'");
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public String ToDebugString(FSM sm) => $"{nameof(NetworkStart)}{m_Role}";
	}
}
