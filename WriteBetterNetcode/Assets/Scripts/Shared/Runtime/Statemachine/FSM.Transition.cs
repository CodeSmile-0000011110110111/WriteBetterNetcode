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
		///     Represents a transition in a statemachine. A transition can also be a self-transition that does not change state.
		/// </summary>
		public sealed class Transition
		{
			public String Name { get; }
			internal ICondition[] Conditions { get; }
			internal IAction[] Actions { get; }
			internal State GotoState { get; }

			private Transition() {} // forbidden default ctor

			/// <summary>
			///     Creates a transition without actions whose purpose is to just change state if its conditions are satisfied.
			/// </summary>
			/// <param name="conditions"></param>
			/// <param name="gotoState"></param>
			public Transition(ICondition[] conditions, State gotoState)
				: this(conditions, null, gotoState) {}

			/// <summary>
			///     Creates a named transition without actions whose purpose is to just change state if its conditions are satisfied.
			/// </summary>
			/// <param name="transitionName"></param>
			/// <param name="conditions"></param>
			/// <param name="gotoState"></param>
			public Transition(String transitionName, ICondition[] conditions, State gotoState)
				: this(transitionName, conditions, null, gotoState) {}

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
				: this(null, conditions, actions, gotoState) {}

			/// <summary>
			///     Creates a named transition that changes state to the given gotoState if conditions are satisfied.
			/// </summary>
			/// <remarks>
			///     The transition name should be used to annotate or summarize the purpose of the transition.
			///     The name is also instrumental for debugging and appears in diagrams.
			/// </remarks>
			/// <param name="transitionName"></param>
			/// <param name="conditions"></param>
			/// <param name="actions"></param>
			/// <param name="gotoState"></param>
			public Transition(String transitionName, ICondition[] conditions, IAction[] actions, State gotoState)
			{
				Name = transitionName ?? String.Empty;
				Conditions = conditions;
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
