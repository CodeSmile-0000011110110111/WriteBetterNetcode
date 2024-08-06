// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Variable.Conditions
{
	public class IsNotEqual : FSM.ICondition
	{
		private readonly FSM.VariableBase m_Variable;
		private readonly FSM.VariableBase m_Comparand;

		private IsNotEqual() {} // forbidden default ctor

		public IsNotEqual(FSM.BoolVar variable, Boolean compareValue)
			: this(variable, new FSM.BoolVar(compareValue)) {}

		public IsNotEqual(FSM.IntVar variable, Int32 compareValue)
			: this(variable, new FSM.IntVar(compareValue)) {}

		public IsNotEqual(FSM.FloatVar variable, Int32 compareValue)
			: this(variable, new FSM.FloatVar(compareValue)) {}

		public IsNotEqual(FSM.FloatVar variable, Single compareValue)
			: this(variable, new FSM.FloatVar(compareValue)) {}

		public IsNotEqual(FSM.BoolVar variable, FSM.BoolVar comparand)
			: this((FSM.VariableBase)variable, comparand) {}

		public IsNotEqual(FSM.IntVar variable, FSM.IntVar comparand)
			: this((FSM.VariableBase)variable, comparand) {}

		public IsNotEqual(FSM.FloatVar variable, FSM.FloatVar comparand)
			: this((FSM.VariableBase)variable, comparand) {}

		private IsNotEqual(FSM.VariableBase variable, FSM.VariableBase comparand)
		{
			m_Variable = variable;
			m_Comparand = comparand;
		}

		public Boolean IsSatisfied(FSM sm) => !m_Variable.Equals(m_Comparand);

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} != {m_Comparand}";
	}
}
