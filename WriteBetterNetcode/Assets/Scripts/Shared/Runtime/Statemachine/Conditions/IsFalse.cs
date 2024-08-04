// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Conditions
{
	public class IsFalse : FSM.ICondition
	{
		private readonly FSM.BoolVariable m_Variable;

		private IsFalse() {} // forbidden default ctor
		public IsFalse(FSM.BoolVariable variable) => m_Variable = variable;

		public Boolean IsSatisfied(FSM sm) => m_Variable.Value == false;

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} == false";
	}
}
