// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		internal sealed class LogicalAndCondition : ICondition
		{
			private readonly ICondition[] m_InnerConditions;
			internal ICondition[] InnerConditions => m_InnerConditions;

			private static void VerifyParameters(ICondition[] andConditions)
			{
#if DEBUG || DEVELOPMENT_BUILD
				if (andConditions == null)
					throw new ArgumentNullException(nameof(andConditions));
				if (andConditions.Length < 2)
					throw new ArgumentException("AND: at least two conditions required!");

				foreach (var cond in andConditions)
				{
					if (cond == null)
						throw new ArgumentNullException($"{nameof(andConditions)} contains null");
				}
#endif
			}

			private LogicalAndCondition() {} // forbidden default ctor

			internal LogicalAndCondition(params ICondition[] andConditions)
			{
				VerifyParameters(andConditions);
				m_InnerConditions = andConditions;
			}

			public Boolean IsSatisfied(FSM sm)
			{
				foreach (var condition in InnerConditions)
				{
					if (condition.IsSatisfied(sm) == false)
						return false;
				}
				return true;
			}
		}
	}
}
