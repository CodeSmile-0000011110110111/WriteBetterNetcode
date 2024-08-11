﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	public sealed class NetworkStop : IAction
	{
		private readonly Boolean m_DiscardMessageQueue;

		private NetworkStop() {}
		public NetworkStop(Boolean discardMessageQueue = false) => m_DiscardMessageQueue = discardMessageQueue;

		public void Execute(FSM sm)
		{
			var net = NetworkManager.Singleton;
			net.Shutdown(m_DiscardMessageQueue);
		}
	}
}
