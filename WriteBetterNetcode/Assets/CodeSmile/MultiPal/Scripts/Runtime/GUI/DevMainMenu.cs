// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Utility;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.Statemachine.Netcode;
using System;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.MultiPal.GUI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(UIDocument))]
	public class DevMainMenu : MenuBase
	{
		private Toggle AllowWebClientsToggle => m_Root.Q<Toggle>("AllowWebClientsToggle");
		private Button HostRelayButton => m_Root.Q<Button>("HostRelayButton");
		private Button HostDirectButton => m_Root.Q<Button>("HostDirectButton");

		private TextField JoinCodeField => m_Root.Q<TextField>("JoinCodeField");
		private Button JoinRelayButton => m_Root.Q<Button>("JoinRelayButton");

		private TextField AddressField => m_Root.Q<TextField>("AddressField");
		private TextField PortField => m_Root.Q<TextField>("PortField");
		private Button JoinDirectButton => m_Root.Q<Button>("JoinDirectButton");

		private static void RequestStart(NetcodeConfig netcodeConfig,
			TransportConfig transportConfig, RelayConfig relayConfig)
		{
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			Debug.Log($"netcodeState: {netcodeState}");

			netcodeState.RequestStart(netcodeConfig, transportConfig, relayConfig);
		}

		protected override void Awake()
		{
			base.Awake();

#if UNITY_WEBGL && !UNITY_EDITOR
			// can't host on the web
			AllowWebClientsToggle.SetEnabled(false);
			HostRelayButton.SetEnabled(false);
			HostDirectButton.SetEnabled(false);
#endif
		}

		private void Start() => RegisterNetcodeStateEvents();
		private void OnDestroy() => UnregisterNetcodeStateEvents();

		private void OnEnable() => RegisterGuiEvents();
		private void OnDisable() => UnregisterGuiEvents();

		private void RegisterNetcodeStateEvents()
		{
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			netcodeState.WentOffline += Show;
			netcodeState.WentOnline += Hide;
		}

		private void UnregisterNetcodeStateEvents()
		{
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			if (netcodeState != null)
			{
				netcodeState.WentOffline -= Show;
				netcodeState.WentOnline -= Hide;
			}
		}

		private void RegisterGuiEvents()
		{
			HostRelayButton.clicked += OnHostRelayButtonClicked;
			HostDirectButton.clicked += OnHostDirectButtonClicked;
			JoinRelayButton.clicked += OnJoinRelayButtonClicked;
			JoinDirectButton.clicked += OnJoinDirectButtonClicked;
			JoinCodeField.RegisterValueChangedCallback(OnJoinCodeChanged);
			AddressField.RegisterValueChangedCallback(OnAddressFieldChanged);
			PortField.RegisterValueChangedCallback(OnPortFieldChanged);
		}

		private void UnregisterGuiEvents()
		{
			HostRelayButton.clicked -= OnHostRelayButtonClicked;
			HostDirectButton.clicked -= OnHostDirectButtonClicked;
			JoinRelayButton.clicked -= OnJoinRelayButtonClicked;
			JoinDirectButton.clicked -= OnJoinDirectButtonClicked;
			JoinCodeField.UnregisterValueChangedCallback(OnJoinCodeChanged);
			AddressField.UnregisterValueChangedCallback(OnAddressFieldChanged);
			PortField.UnregisterValueChangedCallback(OnPortFieldChanged);
		}

		private void OnJoinCodeChanged(ChangeEvent<String> evt)
		{
			JoinCodeField.SetValueWithoutNotify(evt.newValue.ToUpper());

			var textColor = Color.black;
			if (String.IsNullOrWhiteSpace(evt.newValue) ||
			    evt.newValue.Length != 6)
				textColor = Color.red;

			JoinCodeField.style.color = textColor;
		}

		private void OnAddressFieldChanged(ChangeEvent<String> evt)
		{
			var textColor = Color.black;
			if (!IPAddress.TryParse(evt.newValue, out var _))
				textColor = Color.red;

			AddressField.style.color = textColor;
		}

		private void OnPortFieldChanged(ChangeEvent<String> evt)
		{
			var textColor = Color.black;
			if (!UInt16.TryParse(evt.newValue, out var port) ||
			    port < 1024)
				textColor = Color.red;

			PortField.style.color = textColor;
		}

		private void OnHostRelayButtonClicked() => StartHost(true);
		private void OnHostDirectButtonClicked() => StartHost(false);
		private void OnJoinRelayButtonClicked() => JoinWithRelay(JoinCodeField.text);
		private void OnJoinDirectButtonClicked() => JoinWithAddress(AddressField.text, PortField.text);

		private void StartHost(Boolean withRelay)
		{
			var netcodeConfig = NetcodeConfig.FromCmdArgs();
			netcodeConfig.Role = NetcodeRole.Host;

			var transportConfig = TransportConfig.FromNetworkManagerWithCmdArgOverrides();
			transportConfig.ServerListenAddress = "0.0.0.0";
			transportConfig.UseWebSockets = AllowWebClientsToggle.value;

			var relayConfig = RelayConfig.FromCmdArgs();
			relayConfig.UseRelay = withRelay;

			RequestStart(netcodeConfig, transportConfig, relayConfig);
		}

		private void JoinWithRelay(String joinCode)
		{
			var netcodeConfig = NetcodeConfig.FromCmdArgs();
			netcodeConfig.Role = NetcodeRole.Client;

			var transportConfig = TransportConfig.FromNetworkManagerWithCmdArgOverrides();

			var relayConfig = RelayConfig.FromCmdArgs();
			relayConfig.UseRelay = true;
			relayConfig.JoinCode = joinCode;

			RequestStart(netcodeConfig, transportConfig, relayConfig);
		}

		private void JoinWithAddress(String address, String portStr)
		{
			var netcodeConfig = NetcodeConfig.FromCmdArgs();
			netcodeConfig.Role = NetcodeRole.Client;

			var transportConfig = TransportConfig.FromNetworkManagerWithCmdArgOverrides();
			transportConfig.Address = address;
			if (UInt16.TryParse(portStr, out var port))
				transportConfig.Port = port;

			var relayConfig = RelayConfig.FromCmdArgs();

			RequestStart(netcodeConfig, transportConfig, relayConfig);
		}
	}
}