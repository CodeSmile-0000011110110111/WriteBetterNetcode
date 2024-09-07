// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Registry
{
	public class ComponentsRegistry : MonoBehaviour
	{
		public static event Action<Type, Component> OnComponentAssigned;

		private static ComponentsRegistry s_Instance;

		private IComponents m_Components;

		public static T Get<T>() where T : Component => s_Instance.m_Components.Get<T>();

		public static void Set<T>(T component) where T : Component
		{
			s_Instance.m_Components.Set(component);
			OnComponentAssigned?.Invoke(typeof(T), component);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticFields()
		{
			s_Instance = null;
			OnComponentAssigned = null;
		}

		private void Awake()
		{
			if (s_Instance != null)
				throw new InvalidOperationException("ComponentsRegistry already exists!");

			s_Instance = this;

			if (TryGetComponent(out m_Components) == false)
				throw new MissingComponentException($"expected {nameof(IComponents)} component on same object");
		}
	}
}
