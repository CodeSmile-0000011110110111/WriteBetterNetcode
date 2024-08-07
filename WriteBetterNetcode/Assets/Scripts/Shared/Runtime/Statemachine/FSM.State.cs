﻿// Copyright (C) 2021-2024 Steffen Itterheim
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

			internal Transition[] Transitions => m_Transitions;
			public String Name { get; }
			public Boolean Logging { get; set; }

			public static Boolean operator ==(State left, State right) => Equals(left, right);
			public static Boolean operator !=(State left, State right) => !Equals(left, right);

			private State() {} // forbidden default ctor

			public State(String stateName) => Name = stateName;

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
					throw new ArgumentException("transitions null or empty");

				if (m_Transitions == null)
					m_Transitions = transitions;
				else
				{
					var startIndex = m_Transitions.Length;
					var addCount = transitions.Length;
					Array.Resize(ref m_Transitions, startIndex + addCount);

					for (var i = 0; i < addCount; i++)
						m_Transitions[startIndex + i] = transitions[i];
				}

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

			internal void OnStart(FSM sm)
			{
				if (m_Transitions == null)
					m_Transitions = new Transition[0];

				foreach (var transition in Transitions)
					transition.OnStart(sm);
			}

			internal void OnStop(FSM sm)
			{
				foreach (var transition in Transitions)
					transition.OnStop(sm);
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

			public override Boolean Equals(Object obj) => ReferenceEquals(this, obj) || obj is State other && Equals(other);

			public override Int32 GetHashCode() => Name.GetHashCode();
		}
	}
}
