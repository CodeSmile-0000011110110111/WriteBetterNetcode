// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Conditions
{
	public class IsNotEqual : FSM.ICondition
	{
		private readonly FSM.VariableBase m_Variable;
		private readonly FSM.VariableBase m_Comparand;

		private IsNotEqual() {} // forbidden default ctor

		public IsNotEqual(FSM.BoolVariable variable, Boolean compareValue)
			: this(variable, new FSM.BoolVariable(compareValue)) {}

		public IsNotEqual(FSM.IntVariable variable, Int32 compareValue)
			: this(variable, new FSM.IntVariable(compareValue)) {}

		public IsNotEqual(FSM.FloatVariable variable, Int32 compareValue)
			: this(variable, new FSM.FloatVariable(compareValue)) {}

		public IsNotEqual(FSM.FloatVariable variable, Single compareValue)
			: this(variable, new FSM.FloatVariable(compareValue)) {}

		public IsNotEqual(FSM.BoolVariable variable, FSM.BoolVariable comparand)
			: this((FSM.VariableBase)variable, comparand) {}

		public IsNotEqual(FSM.IntVariable variable, FSM.IntVariable comparand)
			: this((FSM.VariableBase)variable, comparand) {}

		public IsNotEqual(FSM.FloatVariable variable, FSM.FloatVariable comparand)
			: this((FSM.VariableBase)variable, comparand) {}

		private IsNotEqual(FSM.VariableBase variable, FSM.VariableBase comparand)
		{
			m_Variable = variable;
			m_Comparand = comparand;
		}

		public Boolean IsSatisfied(FSM sm) => !m_Variable.Equals(m_Comparand);

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} != {sm.GetDebugVarName(m_Comparand)}";
	}
}
