// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		public sealed class LogicalNotCondition : ICondition
		{
			private readonly ICondition m_Condition;

			private LogicalNotCondition() {} // forbidden default ctor

			internal LogicalNotCondition(ICondition condition)
			{
				if (condition == null)
					throw new ArgumentNullException(nameof(condition));

				m_Condition = condition;
			}

			public Boolean IsSatisfied(FSM sm) => !m_Condition.IsSatisfied(sm);
		}
	}
}
