// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Conditions
{
	public class IsLessOrEqual : FSM.ICondition
	{
		private readonly FSM.VariableBase m_Variable;
		private readonly FSM.VariableBase m_Comparand;

		private IsLessOrEqual() {} // forbidden default ctor

		public IsLessOrEqual(FSM.IntVariable variable, Int32 compareValue)
			: this(variable, new FSM.IntVariable(compareValue)) {}

		public IsLessOrEqual(FSM.FloatVariable variable, Single compareValue)
			: this(variable, new FSM.FloatVariable(compareValue)) {}

		public IsLessOrEqual(FSM.FloatVariable variable, Int32 compareValue)
			: this(variable, new FSM.FloatVariable(compareValue)) {}

		public IsLessOrEqual(FSM.IntVariable variable, FSM.IntVariable comparand)
			: this((FSM.VariableBase)variable, comparand) {}

		public IsLessOrEqual(FSM.FloatVariable variable, FSM.FloatVariable comparand)
			: this((FSM.VariableBase)variable, comparand) {}

		private IsLessOrEqual(FSM.VariableBase variable, FSM.VariableBase comparand)
		{
			m_Variable = variable;
			m_Comparand = comparand;
		}

		public Boolean IsSatisfied(FSM sm) => m_Variable <= m_Comparand;

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} <= {sm.GetDebugVarName(m_Comparand)}";
	}
}
