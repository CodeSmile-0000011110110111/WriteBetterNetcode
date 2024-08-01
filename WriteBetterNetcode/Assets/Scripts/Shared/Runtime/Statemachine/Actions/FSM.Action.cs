﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		/// <summary>
		///     Generic Action that simply executes a lambda.
		/// </summary>
		/// <remarks>
		///     This allows for quick and dirty tests but should be avoided for production code, as using a custom
		///     IAction implementation will provide re-usability and make the intent of the Action clear, specifically in logs.
		/// </remarks>
		public sealed class Action : IAction
		{
			private readonly System.Action m_Callback;

			private Action() {} // forbidden default ctor

			public Action(System.Action callback)
			{
				if (callback == null)
					throw new ArgumentNullException(nameof(callback));

				m_Callback = callback;
			}

			public void Execute(FSM sm) => m_Callback.Invoke();

			public override String ToString() => "Lambda Action";
		}
	}
}
