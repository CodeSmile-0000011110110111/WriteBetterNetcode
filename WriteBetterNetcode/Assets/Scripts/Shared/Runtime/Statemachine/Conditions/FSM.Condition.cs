// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		/// <summary>
		///     Generic Condition that simply executes a lambda. Provides static methods for related operators AND, OR, NOT.
		/// </summary>
		/// <remarks>
		///     This allows for quick and dirty tests but should be avoided for production code, as using a custom ICondition
		///     implementation will provide re-usability and make the intent of the Condition clear, specifically in logs.
		/// </remarks>
		public sealed class Condition : ICondition
		{
			private readonly Func<Boolean> m_Condition;

			/// <summary>
			///     Logical NOT operator negates the result of the containing condition.
			/// </summary>
			/// <param name="condition"></param>
			/// <returns></returns>
			public static LogicalNotCondition NOT(ICondition condition) => new(condition);

			/// <summary>
			///     Logical OR operator will be true if one or more of the containing conditions are true.
			/// </summary>
			/// <param name="conditions">Two or more ICondition instances.</param>
			/// <returns></returns>
			public static LogicalOrCondition OR(params ICondition[] conditions) => new(conditions);

			/// <summary>
			///     Logical AND operator will be true if all of the containing conditions are true.
			/// </summary>
			/// <remarks>
			///     Logical AND is the default operation for Conditions. This AND operator is intended to be used within
			///     an OR condition to express more complex conditions like so: OR(AND(a,b), AND(c,d), AND(e,f,g,h))
			/// </remarks>
			/// <param name="conditions">Two or more ICondition instances.</param>
			/// <returns></returns>
			public static LogicalOrCondition AND(params ICondition[] conditions) => new(conditions);

			private Condition() {} // forbidden default ctor

			public Condition(Func<Boolean> condition)
			{
				if (condition == null)
					throw new ArgumentNullException(nameof(condition));

				m_Condition = condition;
			}

			public Boolean IsSatisfied(FSM sm) => m_Condition.Invoke();
		}
	}
}
