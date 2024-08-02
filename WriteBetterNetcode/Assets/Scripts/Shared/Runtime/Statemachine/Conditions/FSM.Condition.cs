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
			private readonly Func<Boolean> m_Callback;

			private Condition() {} // forbidden default ctor

			public Condition(Func<Boolean> callback)
			{
				if (callback == null)
					throw new ArgumentNullException(nameof(callback));

				m_Callback = callback;
			}

			public Boolean IsSatisfied(FSM sm) => m_Callback.Invoke();
		}
	}
}
