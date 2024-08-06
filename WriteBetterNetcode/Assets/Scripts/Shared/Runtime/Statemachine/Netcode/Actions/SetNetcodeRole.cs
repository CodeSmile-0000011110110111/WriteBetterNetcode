// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Actions
{
	public class SetNetcodeRole : IAction
	{
		private readonly StructVar<NetcodeConfig> m_RoleVar;
		private readonly NetcodeRole m_Role;

		private SetNetcodeRole() {} // forbidden ctor

		public SetNetcodeRole(StructVar<NetcodeConfig> roleVar, NetcodeRole role)
		{
			m_RoleVar = roleVar;
			m_Role = role;
		}

		public void Execute(FSM sm)
		{
			var config = m_RoleVar.Value;
			config.Role = m_Role;
			m_RoleVar.Value = config;
		}
	}
}
