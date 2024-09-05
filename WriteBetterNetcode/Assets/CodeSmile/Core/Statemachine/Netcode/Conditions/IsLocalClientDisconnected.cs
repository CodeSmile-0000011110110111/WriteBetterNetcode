// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	public sealed class IsLocalClientDisconnected : IsLocalClientConnected
	{
		public override Boolean IsSatisfied(FSM sm) => !base.IsSatisfied(sm);
	}
}
