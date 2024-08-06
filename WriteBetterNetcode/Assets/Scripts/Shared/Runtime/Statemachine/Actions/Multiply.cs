﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Actions
{
	public class Multiply : FSM.IAction
	{
		private readonly FSM.VariableBase m_Variable;
		private readonly FSM.VariableBase m_Operand;

		private Multiply() {} // forbidden default ctor

		public Multiply(FSM.IntVar variable, Int32 value)
			: this(variable, new FSM.IntVar(value)) {}

		public Multiply(FSM.IntVar variable, FSM.IntVar operand)
			: this((FSM.VariableBase)variable, operand) {}

		public Multiply(FSM.FloatVar variable, Int32 value)
			: this(variable, new FSM.FloatVar(value)) {}

		public Multiply(FSM.FloatVar variable, Single value)
			: this(variable, new FSM.FloatVar(value)) {}

		public Multiply(FSM.FloatVar variable, FSM.FloatVar operand)
			: this((FSM.VariableBase)variable, operand) {}

		private Multiply(FSM.VariableBase variable, FSM.VariableBase operand)
		{
			m_Variable = variable;
			m_Operand = operand;
		}

		public void Execute(FSM sm) => m_Variable.MultiplyValue(m_Operand);

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} = {m_Operand}";
	}
}
