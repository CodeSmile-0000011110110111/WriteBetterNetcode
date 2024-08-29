// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.GUI
{
	[RequireComponent(typeof(UIDocument))]
	public abstract class MenuBase : MonoBehaviour
	{
		protected VisualElement m_Root;

		public Boolean IsHidden => m_Root.style.display == StyleKeyword.None;
		public Boolean IsVisible => !IsHidden;
		public Int32 MenuPlayerIndex { get; set; }

		protected virtual void Awake() => m_Root = GetComponent<UIDocument>().rootVisualElement;

		public void Show()
		{
			m_Root.style.display = StyleKeyword.Initial;
			SetMenuInputEnabled(true);
		}

		public void Hide()
		{
			m_Root.style.display = StyleKeyword.None;
			SetMenuInputEnabled(false);
		}

		public void ToggleVisible()
		{
			if (IsHidden)
				Show();
			else
				Hide();
		}

		private void SetMenuInputEnabled(Boolean menuInputEnabled)
		{
			var inputUsers = Components.InputUsers;
			var playerActions = inputUsers.Actions[MenuPlayerIndex];

			// enable or disable everyone's Player inputs
			foreach (var actions in inputUsers.Actions)
			{
				if (menuInputEnabled)
					actions.Menu.Disable();
				else
					actions.Menu.Enable();
			}

			// enable only the requesting player's UI input
			if (menuInputEnabled)
				playerActions.Menu.Enable();
			else
				playerActions.Menu.Disable();
		}
	}
}
