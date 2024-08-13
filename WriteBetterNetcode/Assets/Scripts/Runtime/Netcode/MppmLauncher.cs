// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode;
using CodeSmile.Statemachine.Netcode;
using System;
using System.Linq;
using Unity.Multiplayer.Playmode;
using UnityEditor;
using UnityEngine;

namespace CodeSmileEditor.BetterNetcode
{
	public class MppmLauncher : MonoBehaviour
	{
#if UNITY_EDITOR
		private void Start()
		{
			var mppmRole = GetNetworkRoleFromMppmTags();
			if (mppmRole != NetcodeRole.None)
			{
				Debug.Log("MPPM NetworkRole is: " + mppmRole);

				var netcodeConfig = new NetcodeConfig { Role = mppmRole, MaxConnections = 0 };
				var transportConfig = TransportConfig.FromNetworkManager();
				Components.NetcodeState.RequestStartNetwork(netcodeConfig, transportConfig);
			}
		}

		private NetcodeRole GetNetworkRoleFromMppmTags()
		{
			var tags = CurrentPlayer.ReadOnlyTags();

			var roleCount = Enum.GetValues(typeof(NetcodeRole)).Length;
			for (var roleIndex = 0; roleIndex < roleCount; roleIndex++)
				if (tags.Contains(((NetcodeRole)roleIndex).ToString()))
					return (NetcodeRole)roleIndex;

			return NetcodeRole.None;
		}
#endif
	}
}
