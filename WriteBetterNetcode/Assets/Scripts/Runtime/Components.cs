// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Input;
using CodeSmile.Netcode;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace CodeSmile
{
	public class Components : MonoBehaviour
	{
		private static Components s_Instance;

		[SerializeField] private NetcodeState m_NetcodeState;
		[SerializeField] private InputUsers m_InputUsers;
		[SerializeField] private InputSystemUIInputModule m_UiInputModule;

		public static NetcodeState NetcodeState => s_Instance?.m_NetcodeState;
		public static InputUsers InputUsers => s_Instance?.m_InputUsers;
		public static InputSystemUIInputModule UiInputModule => s_Instance?.m_UiInputModule;

		private void Awake()
		{
			AssignInstance();
			ThrowIfComponentIsNull();
		}

		private void OnDestroy() => s_Instance = null;

		private void AssignInstance()
		{
			if (s_Instance != null)
				throw new InvalidOperationException("already exists!");

			s_Instance = this;
		}

		private void ThrowIfComponentIsNull()
		{
			if (NetcodeState == null)
				throw new MissingReferenceException($"{nameof(NetcodeState)} not assigned");
			if (m_InputUsers == null)
				throw new MissingReferenceException($"{nameof(InputUsers)} not assigned");
			if (m_UiInputModule == null)
				throw new MissingReferenceException($"{nameof(UiInputModule)} not assigned");
		}
	}
}
