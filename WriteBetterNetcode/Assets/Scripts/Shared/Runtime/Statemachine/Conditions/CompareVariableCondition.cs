// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed class CompareVariableCondition : FSM.ICondition
	{
		public enum Comparator
		{
			Equal,
			NotEqual,
			Greater,
			GreaterOrEqual,
			Less,
			LessOrEqual,
		}

		private readonly FSM.Variable m_Variable;
		private readonly FSM.Variable m_Comparand;
		private readonly Comparator m_Comparator;

		private CompareVariableCondition() {} // forbidden default ctor

		internal CompareVariableCondition(FSM.Variable variable, FSM.Variable comparand,
			Comparator comparator = Comparator.Equal)
		{
#if DEBUG || DEVELOPMENT_BUILD
			if (comparand.Type == FSM.Variable.ValueType.Bool && comparator != Comparator.Equal &&
			    comparator != Comparator.NotEqual)
				throw new ArgumentException($"Bool vars can only be compared for (in)equality, not: {comparator}");
#endif

			m_Variable = variable;
			m_Comparand = comparand;
			m_Comparator = comparator;
		}

		public Boolean IsSatisfied(FSM sm)
		{
			switch (m_Comparator)
			{
				case Comparator.Equal:
					return m_Variable == m_Comparand;
				case Comparator.NotEqual:
					return m_Variable != m_Comparand;
				case Comparator.Greater:
					return m_Variable > m_Comparand;
				case Comparator.GreaterOrEqual:
					return m_Variable >= m_Comparand;
				case Comparator.Less:
					return m_Variable < m_Comparand;
				case Comparator.LessOrEqual:
					return m_Variable <= m_Comparand;
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

			String comp;
			switch (m_Comparator)
			{
				case Comparator.Equal:
					comp = "==";
					break;
				case Comparator.Greater:
					comp = ">";
					break;
				case Comparator.GreaterOrEqual:
					comp = ">=";
					break;
				case Comparator.Less:
					comp = "<";
					break;
				case Comparator.LessOrEqual:
					comp = "<=";
					break;
				default:
					comp = "?";
					break;
			}

			var scope = isGlobal ? "g" : "m";
			return $"'{scope}_{varName}' {comp} {m_Comparand.GetValue()}";
		}
	}
}
