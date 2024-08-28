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

		protected virtual void Awake() => m_Root = GetComponent<UIDocument>().rootVisualElement;
		public virtual void Hide() => m_Root.style.display = StyleKeyword.None;
		public virtual void Show() => m_Root.style.display = StyleKeyword.Initial;
	}
}
