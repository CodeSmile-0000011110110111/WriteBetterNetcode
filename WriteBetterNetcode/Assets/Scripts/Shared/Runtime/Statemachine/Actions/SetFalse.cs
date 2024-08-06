// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Actions
{
	public class SetFalse : FSM.IAction
	{
		private readonly FSM.BoolVar m_Variable;

		private SetFalse() {} // forbidden default ctor
		public SetFalse(FSM.BoolVar variable) => m_Variable = variable;

		public void Execute(FSM sm) => m_Variable.Value = false;

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} = false";
	}
}
