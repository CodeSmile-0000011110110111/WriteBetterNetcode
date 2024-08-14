// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Core.Statemachine.Netcode;
using System;
using System.Linq;
using Unity.Multiplayer.Playmode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.BetterNetcode.Netcode
{
	public class MppmLauncher : MonoBehaviour
	{
#if UNITY_EDITOR
		private void Start()
		{
			var role = GetNetworkRoleFromMppmTags();
			if (role != NetcodeRole.None)
				StartNetworkWithRole(role);
		}

		private static NetcodeRole GetNetworkRoleFromMppmTags()
		{
			var playerTags = CurrentPlayer.ReadOnlyTags();
			var roleCount = Enum.GetValues(typeof(NetcodeRole)).Length;

			for (var roleIndex = 0; roleIndex < roleCount; roleIndex++)
				if (playerTags.Contains(((NetcodeRole)roleIndex).ToString()))
					return (NetcodeRole)roleIndex;

			return NetcodeRole.None;
		}

		private static void StartNetworkWithRole(NetcodeRole role)
		{
			var netcodeConfig = new NetcodeConfig { Role = role, MaxConnections = 4 };
			var transportConfig = TransportConfig.FromNetworkManager();
			Components.NetcodeState.RequestStartNetwork(netcodeConfig, transportConfig);
		}
#endif
	}
}
