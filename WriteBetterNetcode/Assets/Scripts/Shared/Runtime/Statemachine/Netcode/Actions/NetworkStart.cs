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
		private readonly NetworkRole m_Role;

		public NetworkStart(NetworkRole role) => m_Role = role;

		public void Execute(FSM sm)
		{
			try
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
