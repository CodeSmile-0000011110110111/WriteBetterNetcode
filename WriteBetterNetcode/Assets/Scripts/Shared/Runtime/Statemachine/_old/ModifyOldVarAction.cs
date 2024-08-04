// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Enums;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed class ModifyOldVarAction : FSM.IAction
	{
		private readonly FSM.OldVar m_OldVar;
		private readonly FSM.OldVar m_Operand;
		private readonly Operator m_Operator;

		private ModifyOldVarAction() {} // forbidden default ctor

		internal ModifyOldVarAction(FSM.OldVar variable, FSM.OldVar operand, Operator @operator = Operator.Set)
		{
#if DEBUG || DEVELOPMENT_BUILD
			if (operand.Type == FSM.OldVar.ValueType.Bool && @operator != Operator.Set)
				throw new ArgumentException($"Invalid operator for Bool vars: {@operator}");
#endif

			m_OldVar = variable;
			m_Operand = operand;
			m_Operator = @operator;
		}

		public void Execute(FSM sm)
		{
			switch (m_Operator)
			{
				case Operator.Set:
					m_OldVar.Set(m_Operand);
					break;
				case Operator.Add:
					m_OldVar.Add(m_Operand);
					break;
				case Operator.Subtract:
					m_OldVar.Sub(m_Operand);
					break;
				case Operator.Multiply:
					m_OldVar.Mul(m_Operand);
					break;
				case Operator.Divide:
					m_OldVar.Div(m_Operand);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public String ToDebugString(FSM sm)
		{
			var isGlobal = false;
			var varName = sm.OldVars.FindName(m_OldVar);
			if (varName == null)
			{
				isGlobal = true;
				varName = sm.OldGlobalVars.FindName(m_OldVar);
			}

			String op;
			switch (m_Operator)
			{
				case Operator.Set:
					op = "=";
					break;
				case Operator.Add:
					op = "+";
					break;
				case Operator.Subtract:
					op = "-";
					break;
				case Operator.Multiply:
					op = "*";
					break;
				case Operator.Divide:
					op = "/";
					break;
				default:
					op = "?";
					break;
			}

			var scope = isGlobal ? "g" : "m";
			return $"'{scope}_{varName}' {op} {m_Operand.GetValue()}";
		}
	}
}
