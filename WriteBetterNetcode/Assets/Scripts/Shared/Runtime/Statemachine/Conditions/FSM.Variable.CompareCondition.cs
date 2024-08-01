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
			public sealed class CompareCondition : ICondition
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

				private CompareCondition() {} // forbidden default ctor

				internal CompareCondition(Variable variable, Variable comparand, Comparator comparator = Comparator.Equal)
				{
					if (comparand.Type == ValueType.Bool && comparator != Comparator.Equal)
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
							if (m_Variable.Type == ValueType.Float)
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
}
