// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		/// <summary>
		///     Represents a state in a statemachine.
		/// </summary>
		public sealed class State : IEquatable<State>
		{
			private Transition[] m_Transitions;
			public String Name { get; }
			internal Transition[] Transitions { get => m_Transitions; private set => m_Transitions = value; }

			public static Boolean operator ==(State left, State right) => Equals(left, right);
			public static Boolean operator !=(State left, State right) => !Equals(left, right);

			private State() {} // forbidden default ctor

			public State(String stateName)
				: this(stateName, null) {}

			public State(String stateName, Transition[] transitions)
			{
				Name = stateName;
				Transitions = transitions ?? new Transition[0];
			}

			public Boolean Equals(State other)
			{
				if (ReferenceEquals(null, other))
					return false;
				if (ReferenceEquals(this, other))
					return true;

				return Name == other.Name;
			}

			public override String ToString() => $"State({Name})";

			public Transition AddTransition() => AddTransition(new Transition());

			public Transition AddTransition(String transitionName) => AddTransition(new Transition(transitionName));

			public Transition AddTransition(Transition transition)
			{
				AddTransitions(transition);
				return transition;
			}

			public State AddTransitions(params Transition[] transitions)
			{
				if (transitions == null || transitions.Length == 0)
					throw new ArgumentNullException("transitions null or empty");

				var startIndex = m_Transitions.Length;
				var addCount = transitions.Length;
				Array.Resize(ref m_Transitions, startIndex + addCount);

				for (var i = 0; i < addCount; i++)
					m_Transitions[startIndex + i] = transitions[i];

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

			public override Boolean Equals(Object obj) => ReferenceEquals(this, obj) || obj is State other && Equals(other);

			public override Int32 GetHashCode() => Name.GetHashCode();
		}
	}
}
