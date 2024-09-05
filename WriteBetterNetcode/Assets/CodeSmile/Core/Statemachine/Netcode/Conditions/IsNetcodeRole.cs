// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Core.Statemachine.Netcode.Conditions
{
	public class IsNetcodeRole : ICondition
	{
		private readonly Var<NetcodeConfig> m_netcodeConfigVar;
		protected readonly NetcodeRole m_Role;

		private IsNetcodeRole() {} // forbidden ctor

		public IsNetcodeRole(Var<NetcodeConfig> netcodeConfigVar, NetcodeRole role)
		{
			m_netcodeConfigVar = netcodeConfigVar;
			m_Role = role;
		}

		public virtual Boolean IsSatisfied(FSM sm) => m_netcodeConfigVar.Value.Role == m_Role;

		public virtual String ToDebugString(FSM sm) => $"{nameof(IsNetcodeRole)} == {m_Role}";
	}
}
