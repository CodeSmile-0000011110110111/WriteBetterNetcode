﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Actions
{
	public class Divide : FSM.IAction
	{
		private readonly FSM.VariableBase m_Variable;
		private readonly FSM.VariableBase m_Operand;

		private Divide() {} // forbidden default ctor

		public Divide(FSM.IntVariable variable, Int32 value)
			: this(variable, new FSM.IntVariable(value)) {}

		public Divide(FSM.IntVariable variable, FSM.IntVariable operand)
			: this((FSM.VariableBase)variable, operand) {}

		public Divide(FSM.FloatVariable variable, Int32 value)
			: this(variable, new FSM.FloatVariable(value)) {}

		public Divide(FSM.FloatVariable variable, Single value)
			: this(variable, new FSM.FloatVariable(value)) {}

		public Divide(FSM.FloatVariable variable, FSM.FloatVariable operand)
			: this((FSM.VariableBase)variable, operand) {}

		private Divide(FSM.VariableBase variable, FSM.VariableBase operand)
		{
			m_Variable = variable;
			m_Operand = operand;
		}

		public void Execute(FSM sm) => m_Variable.DivideValue(m_Operand);

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} = {sm.GetDebugVarName(m_Operand)}";
	}
}