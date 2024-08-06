// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	public class IsNetcodeRole : FSM.ICondition
	{
		private readonly FSM.StructVar<NetcodeConfig> m_RoleVar;
		private readonly FSM.StructVar<NetcodeConfig> m_CompareVar;

		private IsNetcodeRole() {} // forbidden ctor

		public IsNetcodeRole(FSM.StructVar<NetcodeConfig> roleVar, NetcodeRole role)
			: this(roleVar, new FSM.StructVar<NetcodeConfig>(new NetcodeConfig { Role = role })) {}

		public IsNetcodeRole(FSM.StructVar<NetcodeConfig> roleVar, FSM.StructVar<NetcodeConfig> compareVar)
		{
			m_RoleVar = roleVar;
			m_CompareVar = compareVar;
		}

		public Boolean IsSatisfied(FSM sm) => m_RoleVar.Value.Role == m_CompareVar.Value.Role;
	}
}
