// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Core.Statemachine.Variable.Conditions
{
	public sealed class IsEqual : ICondition
	{
		private readonly VariableBase m_Variable;
		private readonly VariableBase m_Comparand;

		private IsEqual() {} // forbidden default ctor

		public IsEqual(IntVar variable, Int32 compareValue)
			: this(variable, new IntVar(compareValue)) {}

		public IsEqual(FloatVar variable, Int32 compareValue)
			: this(variable, new FloatVar(compareValue)) {}

		public IsEqual(FloatVar variable, Single compareValue)
			: this(variable, new FloatVar(compareValue)) {}

		public IsEqual(BoolVar variable, BoolVar comparand)
			: this((VariableBase)variable, comparand) {}

		public IsEqual(IntVar variable, IntVar comparand)
			: this((VariableBase)variable, comparand) {}

		public IsEqual(FloatVar variable, FloatVar comparand)
			: this((VariableBase)variable, comparand) {}

		private IsEqual(VariableBase variable, VariableBase comparand)
		{
			m_Variable = variable;
			m_Comparand = comparand;
		}

		public Boolean IsSatisfied(FSM sm) => m_Variable.Equals(m_Comparand);

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} == {m_Comparand}";
	}
}
