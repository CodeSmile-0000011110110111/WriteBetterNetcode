// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode;
using CodeSmile.Core.Statemachine.Netcode;
using System;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.GUI
{
	[RequireComponent(typeof(UIDocument))]
	public class DevMainMenu : MonoBehaviour
	{
		private VisualElement m_Root;
		private Button HostRelayButton => m_Root.Q<Button>("HostRelayButton");
		private Button HostDirectButton => m_Root.Q<Button>("HostDirectButton");
		private Button JoinRelayButton => m_Root.Q<Button>("JoinRelayButton");
		private Button JoinDirectButton => m_Root.Q<Button>("JoinDirectButton");
		private TextField JoinCodeField => m_Root.Q<TextField>("JoinCodeField");
		private TextField AddressField => m_Root.Q<TextField>("AddressField");
		private TextField PortField => m_Root.Q<TextField>("PortField");

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
			if (String.IsNullOrWhiteSpace(evt.newValue) || evt.newValue.Trim().Length != 6)
				textColor = Color.red;

			JoinCodeField.style.color = textColor;
		}

		private void OnAddressFieldChanged(ChangeEvent<String> evt)
		{
			var textColor = Color.black;
			if (IPAddress.TryParse(evt.newValue, out var _) == false)
				textColor = Color.red;

			AddressField.style.color = textColor;
		}

		private void OnPortFieldChanged(ChangeEvent<String> evt)
		{
			var textColor = Color.black;
			if (UInt16.TryParse(evt.newValue, out var port) == false || port < 1024)
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

			var relayConfig = RelayConfig.FromCmdArgs();
			relayConfig.UseRelay = withRelay;

			Components.NetcodeState.RequestStartNetwork(netcodeConfig, transportConfig, relayConfig);

			Hide();
		}

		private void JoinWithRelay(String joinCode)
		{
			if (String.IsNullOrWhiteSpace(joinCode) || joinCode.Length != 6)
			{
				Debug.LogWarning($"invalid join code: '{joinCode}'");
				return;
			}

			var netcodeConfig = NetcodeConfig.FromCmdArgs();
			netcodeConfig.Role = NetcodeRole.Client;

			var transportConfig = TransportConfig.FromNetworkManagerWithCmdArgOverrides();

			var relayConfig = RelayConfig.FromCmdArgs();
			relayConfig.UseRelay = true;
			relayConfig.JoinCode = joinCode;

			Components.NetcodeState.RequestStartNetwork(netcodeConfig, transportConfig, relayConfig);

			Hide();
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

			Components.NetcodeState.RequestStartNetwork(netcodeConfig, transportConfig, relayConfig);

			Hide();
		}

		private void Hide() => m_Root.style.display = StyleKeyword.None;
		private void Show() => m_Root.style.display = StyleKeyword.Initial;
	}
}