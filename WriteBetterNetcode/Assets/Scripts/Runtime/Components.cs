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
		private static Components Instance;

		public static NetcodeState NetcodeState => Instance.NetcodeStateComponent;
		[field: SerializeField] private NetcodeState NetcodeStateComponent { get; set; }

		private void Awake()
		{
			if (Instance != null)
				throw new InvalidOperationException("already instantiated!");

			Instance = this;
		}
	}
}
