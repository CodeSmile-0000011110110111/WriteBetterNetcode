// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.FSM
{
	public sealed class Statemachine
	{
		public event Action<StateChangeEventArgs> OnStateChanged;

		private Int32 m_ActiveStateIndex;
		public String Name { get; }
		public State[] States { get; }

		public State ActiveState => States[m_ActiveStateIndex];
		public Boolean IsFinished => ActiveState.IsFinalState();

		internal Boolean DidChangeState { get; private set; }

		public State this[String stateName] => States[FindStateIndexByName(stateName)];

		public Statemachine(String statemachineName, State[] states)
		{
			if (String.IsNullOrWhiteSpace(statemachineName))
				throw new ArgumentException("invalid name", nameof(statemachineName));
			if (states == null || states.Length == 0)
				throw new ArgumentException("no states provided", nameof(states));

			Name = statemachineName;
			States = states;

			ValidateStateNames();
		}

		private void ValidateStateNames()
		{
#if DEBUG || DEVELOPMENT_BUILD
			foreach (var state in States)
			{
				foreach (var transition in state.Transitions)
				{
					if (transition.GotoStateName != null)
						FindStateIndexByName(transition.GotoStateName);
				}
			}
#endif
		}

		public void Update()
		{
			DidChangeState = false;

			var currentState = ActiveState;
			currentState.Update(this);

			if (DidChangeState)
				OnStateChanged?.Invoke(new StateChangeEventArgs { PreviousState = currentState, ActiveState = ActiveState });
		}

		internal void SetActiveState(String stateName)
		{
			ThrowIfAlreadyChangedState();

			var newStateIndex = FindStateIndexByName(stateName);
			DidChangeState = newStateIndex != m_ActiveStateIndex;
			m_ActiveStateIndex = newStateIndex;

			WarnIfStateDidNotChange(stateName);
		}

		private Int32 FindStateIndexByName(String stateName)
		{
			var newStateIndex = Array.FindIndex(States, s => s.Name.Equals(stateName));
			if (newStateIndex < 0)
				throw new ArgumentException($"Statemachine '{Name}' does not contain state named '{stateName}'");

			return newStateIndex;
		}

		private void ThrowIfAlreadyChangedState()
		{
			if (DidChangeState)
				throw new InvalidOperationException("state change already occured this update!");
		}

		private void WarnIfStateDidNotChange(String stateName)
		{
			if (DidChangeState == false)
			{
				Debug.LogWarning($"Statemachine '{Name}' transition changed state to already active state '{stateName}'." +
				                 "If a self-transition was intended, leave the state name empty instead.");
			}
		}

		public struct StateChangeEventArgs
		{
			public State PreviousState;
			public State ActiveState;
		}

		public sealed class State
		{
			public String Name { get; }
			public Transition[] Transitions { get; }

			public State(String stateName)
				: this(stateName, null) {}

			public State(String stateName, Transition[] transitions)
			{
				if (String.IsNullOrWhiteSpace(stateName))
					throw new ArgumentException("invalid name", nameof(stateName));

				Name = stateName;
				Transitions = transitions ?? new Transition[0];
			}

			public Boolean IsFinalState() => Transitions.Length == 0;

			public void Update(Statemachine sm)
			{
				foreach (var transition in Transitions)
				{
					transition.Update(sm);

					// stop evaluating further transitions if the current transition caused a state change
					if (sm.DidChangeState)
						break;
				}
			}
		}

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
				if (ConditionsSatisfied())
				{
					ExecuteActions();
					TryChangeState(sm);
				}
			}

			private Boolean ConditionsSatisfied()
			{
				foreach (var condition in Conditions)
				{
					if (condition.IsSatisfied() == false)
						return false; // early out
				}
				return true;
			}

			private void ExecuteActions()
			{
				foreach (var action in Actions)
					action.Execute();
			}

			private void TryChangeState(Statemachine sm)
			{
				if (GotoStateName != null)
					sm.SetActiveState(GotoStateName);
			}
		}

		public interface ICondition
		{
			public Boolean IsSatisfied();
		}

		public interface IAction
		{
			public void Execute();
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
