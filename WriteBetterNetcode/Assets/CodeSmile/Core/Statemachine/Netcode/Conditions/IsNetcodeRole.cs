// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	/// <summary>
	/// Tests if the config variable's NetcodeRole matches the parameter.
	/// </summary>
	public class IsNetcodeRole : ICondition
	{
		private readonly Var<NetcodeConfig> m_netcodeConfigVar;
		protected readonly NetcodeRole m_Role;

		private IsNetcodeRole() {} // forbidden ctor

		/// <summary>
		/// Creates a new IsNetcodeRole condition.
		/// </summary>
		/// <param name="netcodeConfigVar"></param>
		/// <param name="role"></param>
		public IsNetcodeRole(Var<NetcodeConfig> netcodeConfigVar, NetcodeRole role)
		{
			m_netcodeConfigVar = netcodeConfigVar;
			m_Role = role;
		}

		public virtual Boolean IsSatisfied(FSM sm) => m_netcodeConfigVar.Value.Role == m_Role;

		public virtual String ToDebugString(FSM sm) => $"{nameof(IsNetcodeRole)} == {m_Role}";
	}
}
