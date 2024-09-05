// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Core.Statemachine.Variable.Actions
{
	public sealed class DivValue : IAction
	{
		private readonly VariableBase m_Variable;
		private readonly VariableBase m_Operand;

		private DivValue() {} // forbidden default ctor

		public DivValue(IntVar variable, Int32 value)
			: this(variable, new IntVar(value)) {}

		public DivValue(IntVar variable, IntVar operand)
			: this((VariableBase)variable, operand) {}

		public DivValue(FloatVar variable, Int32 value)
			: this(variable, new FloatVar(value)) {}

		public DivValue(FloatVar variable, Single value)
			: this(variable, new FloatVar(value)) {}

		public DivValue(FloatVar variable, FloatVar operand)
			: this((VariableBase)variable, operand) {}

		private DivValue(VariableBase variable, VariableBase operand)
		{
			m_Variable = variable;
			m_Operand = operand;
		}

		public void Execute(FSM sm) => m_Variable.DivideValue(m_Operand);

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} = {m_Operand}";
	}
}
