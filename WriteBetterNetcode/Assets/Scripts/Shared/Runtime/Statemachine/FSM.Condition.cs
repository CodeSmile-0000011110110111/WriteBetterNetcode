// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		public interface ICondition
		{
			Boolean IsSatisfied(FSM sm);
			void OnStart(FSM sm) {}
			void OnStop(FSM sm) {}
			void OnEnterState(FSM sm) {}
			void OnExitState(FSM sm) {}
		}

		public sealed class Condition : ICondition
		{
			private readonly Func<Boolean> m_Condition;

			private Condition() {} // forbidden default ctor

			public Condition(Func<Boolean> condition)
			{
				if (condition == null)
					throw new ArgumentNullException(nameof(condition));

				m_Condition = condition;
			}

			public Boolean IsSatisfied(FSM sm) => m_Condition.Invoke();
		}

		public sealed class NOT : ICondition
		{
			private readonly ICondition m_Condition;

			private NOT() {} // forbidden default ctor

			public NOT(ICondition condition)
			{
				if (condition == null)
					throw new ArgumentNullException(nameof(condition));

				m_Condition = condition;
			}

			public Boolean IsSatisfied(FSM sm) => !m_Condition.IsSatisfied(sm);
		}

		public sealed class OR : ICondition
		{
			private readonly ICondition[] m_Conditions;

			private OR() {} // forbidden default ctor

			public OR(params ICondition[] conditions)
			{
				if (conditions == null)
					throw new ArgumentNullException(nameof(conditions));
				if (conditions.Length < 2)
					throw new ArgumentException("OR: at least two conditions required!");

				foreach (var condition in conditions)
				{
					if (condition == null)
						throw new ArgumentNullException("conditions must not be null");
				}

				m_Conditions = conditions;
			}

			public Boolean IsSatisfied(FSM sm)
			{
				foreach (var condition in m_Conditions)
				{
					if (condition.IsSatisfied(sm))
						return true;
				}
				return false;
			}
		}

		public sealed class AND : ICondition
		{
			private readonly ICondition[] m_Conditions;

			private AND() {} // forbidden default ctor

			public AND(params ICondition[] conditions)
			{
				if (conditions == null)
					throw new ArgumentNullException(nameof(conditions));
				if (conditions.Length < 2)
					throw new ArgumentException("AND: at least two conditions required!");

				foreach (var condition in conditions)
				{
					if (condition == null)
						throw new ArgumentNullException("conditions must not be null");
				}

				m_Conditions = conditions;
			}

			public Boolean IsSatisfied(FSM sm)
			{
				foreach (var condition in m_Conditions)
				{
					if (condition.IsSatisfied(sm) == false)
						return false;
				}
				return true;
			}
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

			private readonly Variable m_Variable;
			private readonly Variable m_Comparand;
			private readonly Comparator m_Comparator;

			private VariableConditionBase() {} // forbidden default ctor

			internal VariableConditionBase(Variable variable, Variable comparand, Comparator comparator)
			{
				if (comparand.Type == Variable.ValueType.Bool && comparator != Comparator.Equal)
					throw new ArgumentException($"Bool vars can only be compared for equality, not: {comparator}");

				m_Variable = variable;
				m_Comparand = comparand;
				m_Comparator = comparator;
			}

			public Boolean IsSatisfied(FSM sm)
			{
				switch (m_Comparator)
				{
					case Comparator.Equal:
						if (m_Variable.Type == Variable.ValueType.Float)
							return Mathf.Approximately(m_Variable.FloatValue, m_Comparand.FloatValue);

						return m_Variable == m_Comparand;
					case Comparator.GreaterThan:
						return m_Variable > m_Comparand;
					case Comparator.GreaterThanOrEqual:
						return m_Variable >= m_Comparand;
					case Comparator.LessThan:
						return m_Variable < m_Comparand;
					case Comparator.LessThanOrEqual:
						return m_Variable <= m_Comparand;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}
