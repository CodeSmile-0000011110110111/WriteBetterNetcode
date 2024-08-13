// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode;
using CodeSmile.Core.Statemachine.Netcode;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.GUI
{
	[RequireComponent(typeof(UIDocument))]
	public class DevMainMenu : MonoBehaviour
	{
		private VisualElement m_Root;
		private Button HostDirectButton => m_Root.Q<Button>("HostDirectButton");
		private Button HostRelayButton => m_Root.Q<Button>("HostRelayButton");

		private void Awake()
		{
			m_Root = GetComponent<UIDocument>().rootVisualElement;
#if UNITY_WEBGL
			// can't host on the web
			HostRelayButton.SetEnabled(false);
			HostDirectButton.SetEnabled(false);
#endif
		}

		private void Start() => RegisterNetcodeStateEvents();
		private void OnEnable() => RegisterGuiEvents();
		private void OnDisable() => UnregisterGuiEvents();
		private void OnDestroy() => UnregisterNetcodeStateEvents();

		private void RegisterNetcodeStateEvents()
		{
			var netState = Components.NetcodeState;
			netState.WentOffline += Show;
			netState.WentOnline += Hide;
		}

		private void UnregisterNetcodeStateEvents()
		{
			var netState = Components.NetcodeState;
			if (netState != null)
			{
				netState.WentOffline -= Show;
				netState.WentOnline -= Hide;
			}
		}

		private void RegisterGuiEvents()
		{
			HostRelayButton.clicked += OnHostRelayButtonClicked;
			HostDirectButton.clicked += OnHostDirectButtonClicked;
		}

		private void UnregisterGuiEvents()
		{
			HostRelayButton.clicked -= OnHostRelayButtonClicked;
			HostDirectButton.clicked -= OnHostDirectButtonClicked;
		}

		private void OnHostRelayButtonClicked() => StartHost(true);
		private void OnHostDirectButtonClicked() => StartHost(false);

		private void StartHost(Boolean withRelay)
		{
			var netcodeConfig = NetcodeConfig.FromCmdArgs();
			netcodeConfig.Role = NetcodeRole.Host;

			var transportConfig = TransportConfig.FromNetworkManagerWithCmdArgOverrides();
			transportConfig.ServerListenAddress = "0.0.0.0";

			var relayConfig = RelayConfig.FromCmdArgs();
			relayConfig.UseRelay = withRelay;

			var netState = Components.NetcodeState;
			netState.RequestStartNetwork(netcodeConfig, transportConfig, relayConfig);

			Hide();
		}

		private void Hide() => m_Root.style.display = StyleKeyword.None;
		private void Show() => m_Root.style.display = StyleKeyword.Initial;
	}
}
