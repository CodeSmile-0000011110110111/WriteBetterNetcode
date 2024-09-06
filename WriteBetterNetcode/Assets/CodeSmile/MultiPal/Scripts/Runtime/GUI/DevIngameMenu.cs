// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Utility;
using CodeSmile.MultiPal.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.MultiPal.GUI
{
	[DisallowMultipleComponent]
	public class DevIngameMenu : MenuBase
	{
		private Button ResumeButton => m_Root.Q<Button>("ResumeButton");
		private Button ExitMenuButton => m_Root.Q<Button>("ExitMenuButton");
		private Button ExitDesktopButton => m_Root.Q<Button>("ExitDesktopButton");

		private static void ExitPlaymode()
		{
#if UNITY_EDITOR
			EditorApplication.ExitPlaymode();
#endif
		}

		private void Start() => Hide();
		private void OnEnable() => RegisterGuiEvents();
		private void OnDisable() => UnregisterGuiEvents();

		private void RegisterGuiEvents()
		{
			ResumeButton.clicked += OnResumeButtonClicked;
			ExitMenuButton.clicked += OnExitMenuButtonClicked;
			ExitDesktopButton.clicked += OnExitDesktopButtonClicked;
		}

		private void UnregisterGuiEvents()
		{
			ResumeButton.clicked -= OnResumeButtonClicked;
			ExitMenuButton.clicked -= OnExitMenuButtonClicked;
			ExitDesktopButton.clicked -= OnExitDesktopButtonClicked;
		}

		private void OnResumeButtonClicked() => Hide();

		private void OnExitMenuButtonClicked()
		{
			Hide();
			ComponentsRegistry.Get<NetcodeState>().RequestStopNetwork();
		}

		private void OnExitDesktopButtonClicked()
		{
			Application.Quit();

			if (Application.isEditor && Application.isPlaying)
				ExitPlaymode();
		}
	}
}
