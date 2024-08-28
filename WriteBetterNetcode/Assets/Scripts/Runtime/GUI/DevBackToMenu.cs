// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.GUI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(UIDocument))]
	public class DevBackToMenu : MonoBehaviour
	{
		private VisualElement m_Root;
		private Button BackButton => m_Root.Q<Button>("BackButton");

		private void Awake() => m_Root = GetComponent<UIDocument>().rootVisualElement;
		private void Start() => RegisterNetcodeStateEvents();
		private void OnEnable() => RegisterGuiEvents();
		private void OnDisable() => UnregisterGuiEvents();
		private void OnDestroy() => UnregisterNetcodeStateEvents();
		private void RegisterGuiEvents() => BackButton.clicked += OnBackButtonClicked;
		private void UnregisterGuiEvents() => BackButton.clicked -= OnBackButtonClicked;

		private void RegisterNetcodeStateEvents()
		{
			var netState = Components.NetcodeState;
			netState.WentOffline += Hide;
			netState.WentOnline += Show;
		}

		private void UnregisterNetcodeStateEvents()
		{
			var netState = Components.NetcodeState;
			if (netState != null)
			{
				netState.WentOffline -= Hide;
				netState.WentOnline -= Show;
			}
		}

		private void OnBackButtonClicked()
		{
			Debug.Log("Back Button clicked");
			Components.NetcodeState.RequestStopNetwork();
		}

		private void Hide() => m_Root.style.display = StyleKeyword.None;
		private void Show() => m_Root.style.display = StyleKeyword.Initial;
	}
}
