// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.FSM
{
	public sealed partial class Statemachine
	{
		public interface IAction
		{
			public void Execute(Statemachine sm);
		}

		public sealed class Action : IAction
		{
			private readonly System.Action m_Action;

			public Action(System.Action action)
			{
				if (action == null)
					throw new ArgumentNullException(nameof(action));

				m_Action = action;
			}

			public void Execute(Statemachine sm) => m_Action.Invoke();
		}

		public abstract class VariableActionBase : IAction
		{
			public enum Operator
			{
				Set,
				Add,
				Subtract,
				Multiply,
				Divide,
				Negate,
			}

			private readonly String m_VarName;
			private readonly Variable m_Operand;
			private readonly Operator m_Operator;
			private readonly VariableScope m_Scope;

			internal VariableActionBase(String varName, Variable operand, Operator @operator, VariableScope scope)
			{
				if (operand.Type == Variable.ValueType.Bool && @operator != Operator.Set)
					throw new ArgumentException($"Invalid operator for Bool vars: {@operator}");
				if (@operator != Operator.Negate && operand.Type != Variable.ValueType.Bool)
					throw new ArgumentException($"Invalid operator for non-Bool vars: {@operator}");
			}

			public void Execute(Statemachine sm)
			{
				var variable = m_Scope == VariableScope.Local ? sm.LocalVars[m_VarName] : sm.GlobalVars[m_VarName];
				switch (m_Operator)
				{
					case Operator.Set:
						variable.Set(m_Operand);
						break;
					case Operator.Add:
						variable.Add(m_Operand);
						break;
					case Operator.Subtract:
						variable.Sub(m_Operand);
						break;
					case Operator.Multiply:
						variable.Mul(m_Operand);
						break;
					case Operator.Divide:
						variable.Div(m_Operand);
						break;
					case Operator.Negate:
						variable.BoolValue = !variable.BoolValue;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

	}

	public sealed class SetInt : Statemachine.VariableActionBase
	{
		public SetInt(String varName, Int32 value)
			: base(varName, Statemachine.Variable.Int(value), Operator.Set, Statemachine.VariableScope.Local) {}
	}
}
