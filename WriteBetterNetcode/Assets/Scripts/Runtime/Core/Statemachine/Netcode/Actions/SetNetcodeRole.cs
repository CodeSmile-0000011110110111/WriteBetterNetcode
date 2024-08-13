// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Statemachine.Netcode.Actions
{
	public class SetNetcodeRole : IAction
	{
		private readonly Var<NetcodeConfig> m_RoleVar;
		private readonly NetcodeRole m_Role;

		private SetNetcodeRole() {} // forbidden ctor

		public SetNetcodeRole(Var<NetcodeConfig> roleVar, NetcodeRole role)
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

		public String ToDebugString(FSM sm) => $"{nameof(SetNetcodeRole)} = {m_Role}";
	}
}
