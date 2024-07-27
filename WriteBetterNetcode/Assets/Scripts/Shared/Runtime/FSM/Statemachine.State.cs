// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.FSM
{
	public sealed partial class Statemachine
	{
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
	}
}
