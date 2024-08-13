// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	public class IsNotNetcodeRole : ICondition
	{
		private readonly Var<NetcodeConfig> m_RoleVar;
		private readonly NetcodeRole m_Role;

		private IsNotNetcodeRole() {} // forbidden ctor

		public IsNotNetcodeRole(Var<NetcodeConfig> roleVar, NetcodeRole role)
		{
			m_RoleVar = roleVar;
			m_Role = role;
		}

		public Boolean IsSatisfied(FSM sm) => m_RoleVar.Value.Role != m_Role;

		public String ToDebugString(FSM sm) => $"{nameof(IsNetcodeRole)} != {m_Role}";
	}
}
