// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Variable.Actions
{
	public class AddValue : IAction
	{
		private readonly VariableBase m_Variable;
		private readonly VariableBase m_Operand;

		private AddValue() {} // forbidden default ctor

		public AddValue(IntVar variable, Int32 value)
			: this(variable, new IntVar(value)) {}

		public AddValue(IntVar variable, IntVar operand)
			: this((VariableBase)variable, operand) {}

		public AddValue(FloatVar variable, Int32 value)
			: this(variable, new FloatVar(value)) {}

		public AddValue(FloatVar variable, Single value)
			: this(variable, new FloatVar(value)) {}

		public AddValue(FloatVar variable, FloatVar operand)
			: this((VariableBase)variable, operand) {}

		private AddValue(VariableBase variable, VariableBase operand)
		{
			m_Variable = variable;
			m_Operand = operand;
		}

		public void Execute(FSM sm) => m_Variable.AddValue(m_Operand);

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} = {m_Operand}";
	}
}
