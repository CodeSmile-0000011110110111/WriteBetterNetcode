// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.FSM
{
	public sealed partial class Statemachine
	{
		public interface ICondition
		{
			public Boolean IsSatisfied(Statemachine sm);
		}

		public sealed class Condition : ICondition
		{
			private readonly Func<Boolean> m_Condition;

			public Condition(Func<Boolean> condition)
			{
				if (condition == null)
					throw new ArgumentNullException(nameof(condition));

				m_Condition = condition;
			}

			public Boolean IsSatisfied(Statemachine sm) => m_Condition.Invoke();
		}

		public abstract class VariableConditionBase : ICondition
		{
			public enum Comparator
			{
				Equal,
				GreaterThan,
				GreaterThanOrEqual,
				LessThan,
				LessThanOrEqual,
			}

			private readonly String m_VarName;
			private readonly Variable m_Comparand;
			private readonly Comparator m_Comparator;
			private readonly VariableScope m_Scope;

			internal VariableConditionBase(String varName, Variable comparand, Comparator comparator, VariableScope scope)
			{
				if (comparand.Type == Variable.ValueType.Bool && comparator != Comparator.Equal)
					throw new ArgumentException($"Bool vars can only be compared for equality, not: {comparator}");

				m_VarName = varName;
				m_Comparand = comparand;
				m_Comparator = comparator;
				m_Scope = scope;
			}

			public Boolean IsSatisfied(Statemachine sm)
			{
				var variable = m_Scope == VariableScope.Local ? sm.LocalVars[m_VarName] : sm.GlobalVars[m_VarName];
				switch (m_Comparator)
				{
					case Comparator.Equal:
						return variable == m_Comparand;
					case Comparator.GreaterThan:
						return variable > m_Comparand;
					case Comparator.GreaterThanOrEqual:
						return variable >= m_Comparand;
					case Comparator.LessThan:
						return variable < m_Comparand;
					case Comparator.LessThanOrEqual:
						return variable <= m_Comparand;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}

	public sealed class IsEqual : Statemachine.VariableConditionBase
	{
		public IsEqual(String varName, Int32 value)
			: base(varName, Statemachine.Variable.Int(value), Comparator.Equal, Statemachine.VariableScope.Local) {}

		public IsEqual(String varName, Single value)
			: base(varName, Statemachine.Variable.Float(value), Comparator.Equal, Statemachine.VariableScope.Local) {}
	}

	public sealed class IsTrue : Statemachine.VariableConditionBase
	{
		public IsTrue(String varName)
			: base(varName, Statemachine.Variable.Bool(true), Comparator.Equal, Statemachine.VariableScope.Local) {}

	}
	public sealed class IsFalse : Statemachine.VariableConditionBase
	{
		public IsFalse(String varName)
			: base(varName, Statemachine.Variable.Bool(false), Comparator.Equal, Statemachine.VariableScope.Local) {}

	}
}
