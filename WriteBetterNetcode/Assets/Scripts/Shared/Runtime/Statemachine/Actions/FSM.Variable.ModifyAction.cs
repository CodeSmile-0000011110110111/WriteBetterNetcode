// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		public sealed partial class Variable
		{
			public sealed class ModifyVariableAction : IAction
			{
				public enum Operator
				{
					Set,
					Add,
					Subtract,
					Multiply,
					Divide,
				}

				private readonly Variable m_Variable;
				private readonly Variable m_Operand;
				private readonly Operator m_Operator;

				private ModifyVariableAction() {} // forbidden default ctor

				internal ModifyVariableAction(Variable variable, Variable operand, Operator @operator = Operator.Set)
				{
#if DEBUG || DEVELOPMENT_BUILD
					if (operand.Type == ValueType.Bool && @operator != Operator.Set)
						throw new ArgumentException($"Invalid operator for Bool vars: {@operator}");
#endif

					m_Variable = variable;
					m_Operand = operand;
					m_Operator = @operator;
				}

				public void Execute(FSM sm)
				{
					switch (m_Operator)
					{
						case Operator.Set:
							m_Variable.Set(m_Operand);
							break;
						case Operator.Add:
							m_Variable.Add(m_Operand);
							break;
						case Operator.Subtract:
							m_Variable.Sub(m_Operand);
							break;
						case Operator.Multiply:
							m_Variable.Mul(m_Operand);
							break;
						case Operator.Divide:
							m_Variable.Div(m_Operand);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				public String ToDebugString(FSM sm)
				{
					var isGlobal = false;
					var varName = sm.Vars.FindName(m_Variable);
					if (varName == null)
					{
						isGlobal = true;
						varName = sm.GlobalVars.FindName(m_Variable);
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
	}
}
