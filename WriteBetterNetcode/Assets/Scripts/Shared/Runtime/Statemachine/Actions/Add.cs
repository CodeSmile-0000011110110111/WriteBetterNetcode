// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Actions
{
	public class Add : FSM.IAction
	{
		private readonly FSM.VariableBase m_Variable;
		private readonly FSM.VariableBase m_Operand;

		private Add() {} // forbidden default ctor

		public Add(FSM.IntVariable variable, Int32 value)
			: this(variable, new FSM.IntVariable(value)) {}

		public Add(FSM.IntVariable variable, FSM.IntVariable operand)
			: this((FSM.VariableBase)variable, operand) {}

		public Add(FSM.FloatVariable variable, Int32 value)
			: this(variable, new FSM.FloatVariable(value)) {}

		public Add(FSM.FloatVariable variable, Single value)
			: this(variable, new FSM.FloatVariable(value)) {}

		public Add(FSM.FloatVariable variable, FSM.FloatVariable operand)
			: this((FSM.VariableBase)variable, operand) {}

		private Add(FSM.VariableBase variable, FSM.VariableBase operand)
		{
			m_Variable = variable;
			m_Operand = operand;
		}

		public void Execute(FSM sm) => m_Variable.AddValue(m_Operand);

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} = {sm.GetDebugVarName(m_Operand)}";
	}
}
