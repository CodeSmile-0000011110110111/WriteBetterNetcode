// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Registry
{
	[DisallowMultipleComponent]
	public sealed class ComponentsRegistry : MonoBehaviour
	{
		public static event Action<Type, Component> OnComponentAssigned;

		private static Dictionary<Type, Component> s_Components;

		public static T Get<T>() where T : Component =>
			s_Components.ContainsKey(typeof(T)) ? s_Components[typeof(T)] as T : null;

		public static void Set<T>(T component) where T : Component
		{
			if (s_Components == null)
				s_Components = new Dictionary<Type, Component>();

			s_Components[typeof(T)] = component;
			OnComponentAssigned?.Invoke(typeof(T), component);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticFields()
		{
			s_Components = null;
			OnComponentAssigned = null;
		}
	}
}
