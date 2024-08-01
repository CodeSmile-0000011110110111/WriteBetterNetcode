// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		public sealed class LogicalOrCondition : ICondition
		{
			private readonly ICondition[] m_InnerConditions;
			internal ICondition[] InnerConditions => m_InnerConditions;

			private static void VerifyParameters(ICondition[] orConditions)
			{
#if DEBUG || DEVELOPMENT_BUILD
				if (orConditions == null)
					throw new ArgumentNullException(nameof(orConditions));
				if (orConditions.Length < 2)
					throw new ArgumentException("OR: at least two conditions required!");

				foreach (var cond in orConditions)
				{
					if (cond == null)
						throw new ArgumentNullException($"{nameof(orConditions)} contains null");
				}
#endif
			}

			private LogicalOrCondition() {} // forbidden default ctor

			internal LogicalOrCondition(params ICondition[] orConditions)
			{
				VerifyParameters(orConditions);
				m_InnerConditions = orConditions;
			}

			public Boolean IsSatisfied(FSM sm)
			{
				foreach (var condition in InnerConditions)
				{
					if (condition.IsSatisfied(sm))
						return true;
				}
				return false;
			}
		}
	}
}
