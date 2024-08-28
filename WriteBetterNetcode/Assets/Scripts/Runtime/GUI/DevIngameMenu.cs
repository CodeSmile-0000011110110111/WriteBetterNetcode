// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
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

		public Int32 MenuPlayerIndex { get; set; }

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

		private void EnablePlayerUiInput(Boolean uiInputEnabled)
		{
			var inputUsers = Components.InputUsers;
			var playerActions = inputUsers.Actions[MenuPlayerIndex];

			// update everyone's Player inputs
			foreach (var actions in inputUsers.Actions)
			{
				if (uiInputEnabled)
					actions.Player.Disable();
				else
					actions.Player.Enable();

				actions.UI.Disable();
			}

			// enable only the requesting player's UI input
			if (uiInputEnabled)
			{
				playerActions.UI.Enable();
				playerActions.Player.Pause.Enable(); // leave the pause button enabled to dismiss menu
			}
			else
				playerActions.UI.Disable();
		}

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
			Components.NetcodeState.RequestStopNetwork();
		}

		private void OnExitDesktopButtonClicked()
		{
			Application.Quit();

			if (Application.isEditor && Application.isPlaying)
				ExitPlaymode();
		}

		public void ToggleVisible()
		{
			if (IsHidden)
			{
				Show();
				EnablePlayerUiInput(true);
			}
			else
			{
				EnablePlayerUiInput(false);
				OnResumeButtonClicked();
			}
		}
	}
}
