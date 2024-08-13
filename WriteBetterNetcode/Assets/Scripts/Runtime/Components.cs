// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Network;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.BetterNetcode
{
	public class Components : MonoBehaviour
	{
		private static Components s_Instance;

		public static NetcodeState NetcodeState => s_Instance.NetcodeStateComponent;
		[field: SerializeField] private NetcodeState NetcodeStateComponent { get; set; }

		private void Awake()
		{
			AssignInstance();
			ThrowIfComponentIsNull();
		}

		private void AssignInstance()
		{
			if (s_Instance != null)
				throw new InvalidOperationException("already instantiated!");

			s_Instance = this;
		}

		private void ThrowIfComponentIsNull()
		{
			if (NetcodeState == null)
				throw new MissingReferenceException($"{nameof(NetcodeState)} not assigned");
		}
	}
}
