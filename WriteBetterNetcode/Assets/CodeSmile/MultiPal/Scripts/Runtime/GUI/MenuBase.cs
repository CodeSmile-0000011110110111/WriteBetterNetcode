// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Utility;
using CodeSmile.MultiPal.Input;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.MultiPal.GUI
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
			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.AllUiEnabled = menuInputEnabled;
		}
	}
}
