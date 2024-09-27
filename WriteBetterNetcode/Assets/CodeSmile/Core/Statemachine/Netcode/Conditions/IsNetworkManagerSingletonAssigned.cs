// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	/// <summary>
	///     Is true once NetworkManager singleton no longer returns null.
	/// </summary>
	/// <remarks>NetworkManager singleton may be null during Awake/OnEnable depending on the execution order of scripts.</remarks>
	public sealed class IsNetworkManagerSingletonAssigned : ICondition
	{
		public Boolean IsSatisfied(FSM sm) => NetworkManager.Singleton != null;
	}
}
