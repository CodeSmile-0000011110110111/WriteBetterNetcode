// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Conditions
{
	public class IsBoolVarEqual : FSM.ICondition
	{
		private readonly FSM.BoolVariable m_Variable;
		private readonly FSM.BoolVariable m_Comparand;

		private IsBoolVarEqual() {} // forbidden default ctor

		public IsBoolVarEqual(FSM.BoolVariable variable, Boolean compareValue)
			: this(variable, new FSM.BoolVariable(compareValue)) {}

		public IsBoolVarEqual(FSM.BoolVariable variable, FSM.BoolVariable comparand)
		{
			m_Variable = variable;
			m_Comparand = comparand;
		}

		public Boolean IsSatisfied(FSM sm) => m_Variable.Value == m_Comparand.Value;

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} == {m_Comparand.Value}";
	}
}
