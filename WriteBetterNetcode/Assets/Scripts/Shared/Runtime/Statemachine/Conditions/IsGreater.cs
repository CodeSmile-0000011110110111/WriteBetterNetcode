// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Conditions
{
	public class IsGreater : FSM.ICondition
	{
		private readonly FSM.VariableBase m_Variable;
		private readonly FSM.VariableBase m_Comparand;

		private IsGreater() {} // forbidden default ctor

		public IsGreater(FSM.IntVar variable, Int32 compareValue)
			: this(variable, new FSM.IntVar(compareValue)) {}

		public IsGreater(FSM.FloatVar variable, Single compareValue)
			: this(variable, new FSM.FloatVar(compareValue)) {}

		public IsGreater(FSM.FloatVar variable, Int32 compareValue)
			: this(variable, new FSM.FloatVar(compareValue)) {}

		public IsGreater(FSM.IntVar variable, FSM.IntVar comparand)
			: this((FSM.VariableBase)variable, comparand) {}

		public IsGreater(FSM.FloatVar variable, FSM.FloatVar comparand)
			: this((FSM.VariableBase)variable, comparand) {}

		private IsGreater(FSM.VariableBase variable, FSM.VariableBase comparand)
		{
			m_Variable = variable;
			m_Comparand = comparand;
		}

		public Boolean IsSatisfied(FSM sm) => m_Variable > m_Comparand;

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} > {m_Comparand}";
	}
}
