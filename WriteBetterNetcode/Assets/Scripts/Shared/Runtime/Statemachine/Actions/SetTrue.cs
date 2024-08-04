// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Actions
{
	public class SetTrue : FSM.IAction
	{
		private readonly FSM.BoolVariable m_Variable;

		private SetTrue() {} // forbidden default ctor
		public SetTrue(FSM.BoolVariable variable) => m_Variable = variable;

		public void Execute(FSM sm) => m_Variable.Value = true;

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} = true";
	}
}
