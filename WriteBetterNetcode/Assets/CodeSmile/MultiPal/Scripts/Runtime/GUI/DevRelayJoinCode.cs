// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.Statemachine.Netcode;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.MultiPal.GUI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(UIDocument))]
	public class DevRelayJoinCode : MonoBehaviour
	{
		private VisualElement m_Root;
		private Label JoinCodeLabel => m_Root.Q<Label>("JoinCodeLabel");

		private void Awake() => m_Root = GetComponent<UIDocument>().rootVisualElement;

		private void Start() => RegisterNetcodeStateEvents();
		private void OnEnable() => RegisterGuiEvents();
		private void OnDisable() => UnregisterGuiEvents();
		private void OnDestroy() => UnregisterNetcodeStateEvents();

		private void RegisterGuiEvents() => JoinCodeLabel.RegisterCallback<ClickEvent>(OnLabelClicked);

		private void UnregisterGuiEvents() => JoinCodeLabel.UnregisterCallback<ClickEvent>(OnLabelClicked);

		private void OnLabelClicked(ClickEvent evt) => Hide();

		private void RegisterNetcodeStateEvents()
		{
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			netcodeState.WentOffline += WentOffline;
			netcodeState.RelayJoinCodeAvailable += OnRelayJoinCodeAvailable;
		}

		private void UnregisterNetcodeStateEvents()
		{
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			if (netcodeState != null)
			{
				netcodeState.WentOffline -= WentOffline;
				netcodeState.RelayJoinCodeAvailable -= OnRelayJoinCodeAvailable;
			}
		}

		private void WentOffline(NetcodeRole role) => Hide();

		private void OnRelayJoinCodeAvailable(String joinCode)
		{
			JoinCodeLabel.text = joinCode;
			Show();
		}

		private void Hide() => m_Root.style.display = StyleKeyword.None;
		private void Show() => m_Root.style.display = StyleKeyword.Initial;
	}
}
