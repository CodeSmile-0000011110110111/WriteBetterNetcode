// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	public sealed class IsNetworkOffline : FSM.ICondition
	{
		public Boolean IsSatisfied(FSM sm)
		{
			var net = NetworkManager.Singleton;
			if (net == null)
				return false;

			return !(net.ShutdownInProgress || net.IsListening || net.IsServer || net.IsHost || net.IsClient);
		}

		public override String ToString() => nameof(IsNetworkOffline);
	}
}
