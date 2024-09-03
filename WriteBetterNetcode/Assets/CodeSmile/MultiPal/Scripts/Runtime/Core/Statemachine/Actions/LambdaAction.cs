// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Statemachine.Actions
{
	/// <summary>
	///     Generic Action that simply executes a lambda.
	/// </summary>
	/// <remarks>
	///     This allows for quick and dirty tests but should be avoided for production code, as using a custom
	///     IAction implementation will provide re-usability and make the intent of the Action clear, specifically in logs.
	/// </remarks>
	public sealed class LambdaAction : IAction
	{
		private readonly String m_Name;
		private readonly Action m_Callback;

		private LambdaAction() {} // forbidden default ctor

		public LambdaAction(Action callback)
			: this(null, callback) {}

		public LambdaAction(String name, Action callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			m_Name = name;
			m_Callback = callback;
		}

		public void Execute(FSM sm) => m_Callback.Invoke();

		public String ToDebugString(FSM sm) => String.IsNullOrWhiteSpace(m_Name) ? GetType().Name : m_Name;
	}
}
