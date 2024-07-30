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
		/// Represents a state in a statemachine.
		/// </summary>
		public sealed class State
		{
			public String Name { get; }
			internal Transition[] Transitions { get; private set; }

			public State(String stateName)
			{
				if (String.IsNullOrWhiteSpace(stateName))
					throw new ArgumentException("invalid name", nameof(stateName));

				Name = stateName;
				Transitions = new Transition[0];
			}

			public State WithTransitions(Transition[] transitions)
			{
				if (Transitions.Length > 0)
					throw new InvalidOperationException("transitions already set");

				Transitions = transitions ?? new Transition[0];
				return this;
			}

			public Boolean IsFinalState() => Transitions.Length == 0;

			public void Update(FSM sm)
			{
				foreach (var transition in Transitions)
				{
					transition.Update(sm);

					// stop evaluating further transitions if the current transition caused a state change
					if (sm.DidChangeState)
						break;
				}
			}

			internal void OnExitState(FSM sm)
			{
				foreach (var transition in Transitions)
					transition.OnExitState(sm);
			}

			internal void OnEnterState(FSM sm)
			{
				foreach (var transition in Transitions)
					transition.OnEnterState(sm);
			}

			internal void OnStart(FSM sm)
			{
				foreach (var transition in Transitions)
					transition.OnStart(sm);
			}

			internal void OnStop(FSM sm)
			{
				foreach (var transition in Transitions)
					transition.OnStop(sm);
			}
		}
	}
}
