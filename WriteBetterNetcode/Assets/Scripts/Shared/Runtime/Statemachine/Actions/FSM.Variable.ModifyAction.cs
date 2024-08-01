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
			public sealed class ModifyAction : IAction
			{
				public enum Operator
				{
					Set,
					Add,
					Subtract,
					Multiply,
					Divide,
					// Negate,
				}

				private readonly Variable m_Variable;
				private readonly Variable m_Operand;
				private readonly Operator m_Operator;

				private ModifyAction() {} // forbidden default ctor

				internal ModifyAction(Variable variable, Variable operand, Operator @operator = Operator.Set)
				{
					if (operand.Type == ValueType.Bool && @operator != Operator.Set)
						throw new ArgumentException($"Invalid operator for Bool vars: {@operator}");
					// if (@operator == Operator.Negate && operand.Type != ValueType.Bool)
					// 	throw new ArgumentException($"Invalid operator for non-Bool vars: {@operator}");

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
						// case Operator.Negate:
						// 	m_Variable.BoolValue = !m_Variable.BoolValue;
						// 	break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
		}
	}
}
