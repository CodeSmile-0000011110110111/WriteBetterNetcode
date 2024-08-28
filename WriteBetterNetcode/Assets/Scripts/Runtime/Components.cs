// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Input;
using CodeSmile.Netcode;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	public class Components : MonoBehaviour
	{
		private static Components s_Instance;

		[SerializeField] private NetcodeState m_NetcodeState;
		[SerializeField] private InputUsers m_InputUsers;

		public static NetcodeState NetcodeState => s_Instance?.m_NetcodeState;
		public static InputUsers InputUsers => s_Instance?.m_InputUsers;

		private void Awake()
		{
			AssignInstance();

			ThrowIfNotAssigned<NetcodeState>(m_NetcodeState);
			ThrowIfNotAssigned<InputUsers>(m_InputUsers);
		}

		private void OnDestroy() => s_Instance = null;

		private void AssignInstance()
		{
			if (s_Instance != null)
				throw new InvalidOperationException("already exists!");

			s_Instance = this;
		}

		private void ThrowIfNotAssigned<T>(Component component) where T : Component
		{
			if (component == null || component is not T)
				throw new MissingReferenceException($"{typeof(T).Name} not assigned");
		}
	}
}
