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
			public Boolean IsSatisfied();
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

			public Boolean IsSatisfied() => m_Condition.Invoke();
		}
	}
}
