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
				StartNetworkWithRole(netcodeConfig);
		}

		private static void StartNetworkWithRole(NetcodeConfig netcodeCfg)
		{
			var transportCfg = TransportConfig.FromNetworkManagerWithCmdArgOverrides();
			var relayCfg = RelayConfig.FromCmdArgs();
			Components.NetcodeState.RequestStartNetwork(netcodeCfg, transportCfg, relayCfg);
		}
	}
}
