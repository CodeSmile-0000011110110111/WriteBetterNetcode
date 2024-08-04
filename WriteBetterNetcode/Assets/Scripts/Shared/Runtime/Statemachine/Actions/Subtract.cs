// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Actions
{
	public class Subtract : FSM.IAction
	{
		private readonly FSM.VariableBase m_Variable;
		private readonly FSM.VariableBase m_Operand;

		private Subtract() {} // forbidden default ctor

		public Subtract(FSM.IntVariable variable, Int32 value)
			: this(variable, new FSM.IntVariable(value)) {}

		public Subtract(FSM.IntVariable variable, FSM.IntVariable operand)
			: this((FSM.VariableBase)variable, operand) {}

		public Subtract(FSM.FloatVariable variable, Int32 value)
			: this(variable, new FSM.FloatVariable(value)) {}

		public Subtract(FSM.FloatVariable variable, Single value)
			: this(variable, new FSM.FloatVariable(value)) {}

		public Subtract(FSM.FloatVariable variable, FSM.FloatVariable operand)
			: this((FSM.VariableBase)variable, operand) {}

		private Subtract(FSM.VariableBase variable, FSM.VariableBase operand)
		{
			m_Variable = variable;
			m_Operand = operand;
		}

		public void Execute(FSM sm) => m_Variable.SubtractValue(m_Operand);

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)} = {sm.GetDebugVarName(m_Operand)}";
	}
}
