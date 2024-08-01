// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
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

			public static NotCondition NOT(ICondition condition) => new NotCondition(condition);
		}

		public sealed class NotCondition : ICondition
		{
			private readonly ICondition m_Condition;

			private NotCondition() {} // forbidden default ctor

			internal NotCondition(ICondition condition)
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

	}
}
