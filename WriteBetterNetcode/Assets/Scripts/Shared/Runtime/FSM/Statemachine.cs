// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.FSM
{
	public sealed partial class Statemachine
	{
		public event Action<StateChangeEventArgs> OnStateChanged;

		// Global (world) variables
		private static readonly Variables s_GlobalVars = new();

		private readonly String m_Name;
		private readonly State[] m_States;

		// Per statemachine variables
		private readonly Variables m_LocalVars = new();

		private Int32 m_ActiveStateIndex;

		internal Boolean DidChangeState { get; private set; }

		public String Name => m_Name;
		public State ActiveState => m_States[m_ActiveStateIndex];
		public Boolean IsFinished => ActiveState.IsFinalState();

		public State this[String stateName] => m_States[FindStateIndexByName(stateName)];

		public Statemachine(String statemachineName, State[] states)
		{
			if (String.IsNullOrWhiteSpace(statemachineName))
				throw new ArgumentException("invalid name", nameof(statemachineName));
			if (states == null || states.Length == 0)
				throw new ArgumentException("no states provided", nameof(states));

			m_Name = statemachineName;
			m_States = states;

			ValidateStateNames();
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
			var newStateIndex = Array.FindIndex(m_States, s => s.Name.Equals(stateName));
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

		private void ValidateStateNames()
		{
#if DEBUG || DEVELOPMENT_BUILD
			foreach (var state in m_States)
			{
				foreach (var transition in state.Transitions)
				{
					if (transition.GotoStateName != null)
						FindStateIndexByName(transition.GotoStateName);
				}
			}
#endif
		}

		public struct StateChangeEventArgs
		{
			public State PreviousState;
			public State ActiveState;
		}
	}
}
