// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	public class NetworkManagerStartServer : FSM.IAction
	{
		public void Execute(FSM sm) => NetworkManager.Singleton.StartServer();
	}
}
