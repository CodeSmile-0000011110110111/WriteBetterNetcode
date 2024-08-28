// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.GUI
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

		protected override void Awake()
		{
			base.Awake();
			Hide();
		}

		private void OnEnable() => RegisterGuiEvents();
		private void OnDisable() => UnregisterGuiEvents();

		private void RegisterGuiEvents()
		{
			ResumeButton.clicked += Hide;
			ExitMenuButton.clicked += OnExitMenuButtonClicked;
			ExitDesktopButton.clicked += OnExitDesktopButtonClicked;
		}

		private void UnregisterGuiEvents()
		{
			ResumeButton.clicked -= Hide;
			ExitMenuButton.clicked -= OnExitMenuButtonClicked;
			ExitDesktopButton.clicked -= OnExitDesktopButtonClicked;
		}

		private void OnExitMenuButtonClicked()
		{
			Hide();
			Components.NetcodeState.RequestStopNetwork();
		}

		private void OnExitDesktopButtonClicked()
		{
			Application.Quit();

			if (Application.isEditor && Application.isPlaying)
				ExitPlaymode();
		}
	}
}
