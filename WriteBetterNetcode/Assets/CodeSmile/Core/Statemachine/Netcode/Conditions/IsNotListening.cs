// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	/// <summary>
	///     Is true if the NetworkManager IsListening state is false (offline).
	/// </summary>
	public sealed class IsNotListening : ICondition
	{
		public Boolean IsSatisfied(FSM sm) => !NetworkManager.Singleton.IsListening;
	}
}
