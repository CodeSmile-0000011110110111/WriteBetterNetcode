// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	public class NetworkManagerShutdown : FSM.IAction
	{
		private readonly Boolean m_DiscardMessageQueue;

		private NetworkManagerShutdown() {}
		public NetworkManagerShutdown(Boolean discardMessageQueue = false) => m_DiscardMessageQueue = discardMessageQueue;

		public void Execute(FSM sm) => NetworkManager.Singleton.Shutdown(m_DiscardMessageQueue);

		public String ToDebugString(FSM sm) => $"NetworkManager.Shutdown({m_DiscardMessageQueue})";
	}
}
