// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.FSM
{
	public sealed partial class Statemachine
	{
		public interface IAction
		{
			public void Execute();
		}

		public sealed class Action : IAction
		{
			private readonly System.Action m_Action;

			public Action(System.Action action)
			{
				if (action == null)
					throw new ArgumentNullException(nameof(action));

				m_Action = action;
			}

			public void Execute() => m_Action.Invoke();
		}
	}
}
