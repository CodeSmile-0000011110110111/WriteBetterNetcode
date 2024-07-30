// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.FSM
{
	public sealed partial class Statemachine
	{
		public sealed class Transition
		{
			public ICondition[] Conditions { get; }
			public IAction[] Actions { get; }
			public String GotoStateName { get; }

			public Transition(ICondition[] conditions, IAction[] actions)
				: this(conditions, actions, null) {}

			public Transition(ICondition[] conditions, IAction[] actions, String gotoStateName)
			{
				Conditions = conditions ?? new ICondition[0];
				Actions = actions ?? new IAction[0];
				GotoStateName = gotoStateName;
			}

			public void Update(Statemachine sm)
			{
				if (ConditionsSatisfied(sm))
				{
					ExecuteActions(sm);
					TryChangeState(sm);
				}
			}

			private Boolean ConditionsSatisfied(Statemachine sm)
			{
				foreach (var condition in Conditions)
				{
					if (condition.IsSatisfied(sm) == false)
						return false; // early out
				}
				return true;
			}

			private void ExecuteActions(Statemachine sm)
			{
				foreach (var action in Actions)
					action.Execute(sm);
			}

			private void TryChangeState(Statemachine sm)
			{
				if (GotoStateName != null)
					sm.SetActiveState(GotoStateName);
			}
		}
	}
}
