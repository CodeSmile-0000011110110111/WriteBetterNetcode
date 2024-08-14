// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Statemachine.Netcode.Conditions
{
	public class IsNetcodeRole : ICondition
	{
		private readonly Var<NetcodeConfig> m_netcodeConfigVar;
		private readonly NetcodeRole m_Role;

		private IsNetcodeRole() {} // forbidden ctor

		public IsNetcodeRole(Var<NetcodeConfig> netcodeConfigVar, NetcodeRole role)
		{
			m_netcodeConfigVar = netcodeConfigVar;
			m_Role = role;
		}

		public Boolean IsSatisfied(FSM sm) => m_netcodeConfigVar.Value.Role == m_Role;

		public String ToDebugString(FSM sm) => $"{nameof(IsNetcodeRole)} == {m_Role}";
	}
}
