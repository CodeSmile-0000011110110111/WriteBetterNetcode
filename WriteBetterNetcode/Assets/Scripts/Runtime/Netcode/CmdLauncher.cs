// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Netcode;
using CodeSmile.Statemachine.Services;
using CodeSmile.Utility;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.BetterNetcode
{
	public class CmdLauncher : MonoBehaviour
	{
		private void Start()
		{
			CmdArgs.Log();

			var netcodeConfig = NetcodeConfig.FromCmdArgs();
			if (netcodeConfig.Role != NetcodeRole.None)
			{
				var transportConfig = TransportConfig.FromNetworkManagerWithCmdArgOverrides();
				var relayConfig = RelayConfig.FromCmdArgs();
				Components.NetcodeState.RequestStartNetwork(netcodeConfig, transportConfig, relayConfig);
			}
		}
	}
}
