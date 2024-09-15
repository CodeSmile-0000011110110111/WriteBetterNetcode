// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.Statemachine.Netcode;
using CodeSmile.Utility;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Netcode
{
	[DisallowMultipleComponent]
	internal sealed class CmdLauncher : MonoBehaviour
	{
		private static Boolean m_DidLaunchOnce;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticFields() => m_DidLaunchOnce = false;

		private static void StartNetworkWithRole(NetcodeConfig netcodeCfg)
		{
			var transportCfg = TransportConfig.FromNetworkManagerWithCmdArgOverrides();
			var relayCfg = RelayConfig.FromCmdArgs();

			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			netcodeState.RequestStart(netcodeCfg, transportCfg, relayCfg);
		}

		private void Start()
		{
			if (m_DidLaunchOnce)
				return;

			m_DidLaunchOnce = true;

			if (Application.isEditor == false)
				CmdArgs.Log();

			var netcodeConfig = NetcodeConfig.FromCmdArgs();
			if (netcodeConfig.Role != NetcodeRole.None)
				StartNetworkWithRole(netcodeConfig);
		}
	}
}
