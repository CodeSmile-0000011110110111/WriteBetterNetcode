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
			private readonly ICondition[] m_Conditions;

			private LogicalAndCondition() {} // forbidden default ctor

			internal LogicalAndCondition(params ICondition[] conditions)
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
	}
}
