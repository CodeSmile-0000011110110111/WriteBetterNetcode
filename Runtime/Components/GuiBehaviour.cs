// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.Components
{
	public class GuiBehaviour : MonoBehaviour
	{
		private const String PanelContentRootName = "panel-content";

		[SerializeField] private VisualTreeAsset m_ContentSourceAsset;

		protected UIDocument m_Document;
		protected VisualElement m_Root;

		protected virtual void OnEnable()
		{
			m_Document = GetComponent<UIDocument>();
			m_Root = m_Document.rootVisualElement;

			if (m_ContentSourceAsset != null)
			{
				var contentRoot = FindFirst<VisualElement>(PanelContentRootName);
				m_ContentSourceAsset.CloneTree(contentRoot);
			}

			OnRegisterEvents();
			OnShowGUI();
		}

		protected virtual void OnDisable()
		{
			OnHideGUI();
			OnUnregisterEvents();
		}

		protected virtual void OnDestroy() => OnDestroyGUI();
		protected virtual void OnDestroyGUI() {}
		protected virtual void OnShowGUI() {}
		protected virtual void OnHideGUI() {}
		protected virtual void OnRegisterEvents() {}
		protected virtual void OnUnregisterEvents() {}

		protected T FindFirst<T>(String name, VisualElement element = null) where T : VisualElement
		{
			var found = element == null ? m_Root?.Q<T>(name) : element.Q<T>(name);
			if (found == null)
				throw new ArgumentException($"not found: '{name}' in {element}/{m_Root}");

			return found;
		}

		protected List<T> FindAll<T>(String name, VisualElement element = null) where T : VisualElement =>
			element == null ? m_Root.Query<T>(name).ToList() : element.Query<T>(name).ToList();
	}
}
