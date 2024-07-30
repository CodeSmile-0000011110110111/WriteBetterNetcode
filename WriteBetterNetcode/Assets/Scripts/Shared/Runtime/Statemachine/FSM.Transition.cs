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
		/// Represents a transition in a statemachine. A transition can also be a self-transition that does not change state.
		/// </summary>
		public sealed class Transition
		{
			internal ICondition[] Conditions { get; }
			internal IAction[] Actions { get; }
			internal State GotoState { get; }

			/// <summary>
			///     Creates a self-transition that does not change the current state if its conditions are satisfied.
			/// </summary>
			/// <param name="conditions"></param>
			/// <param name="actions"></param>
			public Transition(ICondition[] conditions, IAction[] actions)
				: this(conditions, actions, null) {}

			/// <summary>
			///     Creates a transition that changes state to the given gotoState if conditions are satisfied.
			/// </summary>
			/// <param name="conditions"></param>
			/// <param name="actions"></param>
			/// <param name="gotoState"></param>
			public Transition(ICondition[] conditions, IAction[] actions, State gotoState)
			{
				Conditions = conditions ?? new ICondition[0];
				Actions = actions ?? new IAction[0];
				GotoState = gotoState;
			}

			internal void Update(FSM sm)
			{
				if (ConditionsSatisfied(sm))
				{
					ExecuteActions(sm);

					if (GotoState != null)
						sm.SetActiveState(GotoState);
				}
			}

			private Boolean ConditionsSatisfied(FSM sm)
			{
				foreach (var condition in Conditions)
				{
					if (condition.IsSatisfied(sm) == false)
						return false; // early out
				}
				return true;
			}

			private void ExecuteActions(FSM sm)
			{
				foreach (var action in Actions)
					action.Execute(sm);
			}

			internal void OnExitState(FSM sm)
			{
				foreach (var condition in Conditions)
					condition.OnExitState(sm);
				foreach (var action in Actions)
					action.OnExitState(sm);
			}

			internal void OnEnterState(FSM sm)
			{
				foreach (var condition in Conditions)
					condition.OnEnterState(sm);
				foreach (var action in Actions)
					action.OnEnterState(sm);
			}

			internal void OnStart(FSM sm)
			{
				foreach (var condition in Conditions)
					condition.OnStart(sm);
				foreach (var action in Actions)
					action.OnStart(sm);
			}

			internal void OnStop(FSM sm)
			{
				foreach (var condition in Conditions)
					condition.OnStop(sm);
				foreach (var action in Actions)
					action.OnStop(sm);
			}
		}
	}
}
