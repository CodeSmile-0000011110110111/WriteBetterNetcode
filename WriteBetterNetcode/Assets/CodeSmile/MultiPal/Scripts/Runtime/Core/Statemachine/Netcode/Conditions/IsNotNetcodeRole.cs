// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Statemachine.Netcode.Conditions
{
	public class IsNotNetcodeRole : IsNetcodeRole
	{
		public IsNotNetcodeRole(Var<NetcodeConfig> netcodeConfigVar, NetcodeRole role)
			: base(netcodeConfigVar, role) {}

		public override Boolean IsSatisfied(FSM sm) => !base.IsSatisfied(sm);

		public override String ToDebugString(FSM sm) => $"{nameof(IsNetcodeRole)} != {m_Role}";
	}
}
