// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	public sealed class NetworkStart : FSM.IAction
	{
		private readonly NetcodeRole m_Role;

		public NetworkStart(NetcodeRole role) => m_Role = role;

		public void Execute(FSM sm)
		{
			try
			{
				switch (m_Role)
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
						throw new ArgumentOutOfRangeException(nameof(m_Role), m_Role.ToString());
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"NetworkStart {m_Role} failed: {e}");
			}
		}

		public String ToDebugString(FSM sm) => $"{nameof(NetworkStart)}{m_Role}";
	}
}
