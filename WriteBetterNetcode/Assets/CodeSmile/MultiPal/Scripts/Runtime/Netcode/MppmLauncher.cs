// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.Statemachine.Netcode;
using System;
using System.Linq;
using Unity.Multiplayer.Playmode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Netcode
{
	internal class MppmLauncher : MonoBehaviour
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
			{
				if (playerTags.Contains(((NetcodeRole)roleIndex).ToString()))
					return (NetcodeRole)roleIndex;
			}

			return NetcodeRole.None;
		}

		private static void StartNetworkWithRole(NetcodeRole role)
		{
			var netcodeConfig = new NetcodeConfig { Role = role, MaxConnections = 4 };
			var transportConfig = TransportConfig.FromNetworkManager();

			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			netcodeState.RequestStart(netcodeConfig, transportConfig);
		}

		// ensure unsaved Material, ScriptableObject, etc changes are applied to virtual players
		[InitializeOnLoadMethod] private static void InitOnLoad() => EditorApplication.playModeStateChanged += state =>
		{
			if (state == PlayModeStateChange.ExitingEditMode)
				SaveProject();
		};

		private static void SaveProject() => AssetDatabase.SaveAssets();
#endif
	}
}
