// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Core.Statemachine.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.BetterNetcode.Netcode
{
	public class CmdLauncher : MonoBehaviour
	{
		private void Start()
		{
#if !UNITY_EDITOR
			CmdArgs.Log();
#endif

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
