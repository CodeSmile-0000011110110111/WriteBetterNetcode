// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	/// <summary>
	///     Tests if network is fully offline - the server/host is not immediately offline after calling Shutdown().
	/// </summary>
	/// <remarks>
	///     Technically, this checks if NetworkManager singleton is null and if not, whether ShutdownInProgress, IsListening,
	///     IsServer, IsHost, IsClient are all false. It may take a few frames for all of these to reset and during this state
	///     you cannot start a new NetworkManager session.
	/// </remarks>
	public sealed class IsNetworkOffline : ICondition
	{
		public Boolean IsSatisfied(FSM sm)
		{
			var net = NetworkManager.Singleton;
			if (net == null)
				return false;

			return !(net.ShutdownInProgress || net.IsListening ||
			         net.IsServer || net.IsHost || net.IsClient);
		}
	}
}
