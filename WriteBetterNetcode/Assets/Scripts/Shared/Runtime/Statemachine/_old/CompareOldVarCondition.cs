// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Enums;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed class CompareOldVarCondition : FSM.ICondition
	{
		private readonly FSM.OldVar m_OldVar;
		private readonly FSM.OldVar m_Comparand;
		private readonly Comparator m_Comparator;

		private CompareOldVarCondition() {} // forbidden default ctor

		internal CompareOldVarCondition(FSM.OldVar variable, FSM.OldVar comparand,
			Comparator comparator = Comparator.Equal)
		{
#if DEBUG || DEVELOPMENT_BUILD
			if (comparand.Type == FSM.OldVar.ValueType.Bool && comparator != Comparator.Equal &&
			    comparator != Comparator.NotEqual)
				throw new ArgumentException($"Bool vars can only be compared for (in)equality, not: {comparator}");
#endif

			m_OldVar = variable;
			m_Comparand = comparand;
			m_Comparator = comparator;
		}

		public Boolean IsSatisfied(FSM sm)
		{
			switch (m_Comparator)
			{
				case Comparator.Equal:
					return m_OldVar == m_Comparand;
				case Comparator.NotEqual:
					return m_OldVar != m_Comparand;
				case Comparator.Greater:
					return m_OldVar > m_Comparand;
				case Comparator.GreaterOrEqual:
					return m_OldVar >= m_Comparand;
				case Comparator.Less:
					return m_OldVar < m_Comparand;
				case Comparator.LessOrEqual:
					return m_OldVar <= m_Comparand;
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
