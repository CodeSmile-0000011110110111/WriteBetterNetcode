// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	/// <summary>
	///     Represents a finite-statemachine with a set of local and global variables indexed by name.
	/// </summary>
	public sealed partial class FSM
	{
		/// <summary>
		///     Invoked after each state change.
		/// </summary>
		public event Action<StateChangeEventArgs> OnStateChange;

		/// <summary>
		///     Invoked when the statemachine entered a final state (state without any transitions).
		/// </summary>
		public event Action<StatemachineStoppedEventArgs> OnStopped;

		private static readonly Variables s_GlobalVars = new();

		private State[] m_States;

		private Int32 m_ActiveStateIndex = -1;
		private Boolean Started => !(m_ActiveStateIndex < 0);

		/// <summary>
		///     Name of the Statemachine
		/// </summary>
		public String Name { get; }

		/// <summary>
		///     The currently active State.
		/// </summary>
		/// <remarks>Only use this after calling Start().</remarks>
		public State ActiveState
		{
			get
			{
				ThrowIfStatemachineNotStarted();
				return m_States[m_ActiveStateIndex];
			}
		}

		/// <summary>
		///     Global variables are available for all statemachines (be mindful!).
		/// </summary>
		public Variables GlobalVars => s_GlobalVars;

		/// <summary>
		///     These variables are unique to the Statemachine instance.
		/// </summary>
		public Variables Vars { get; } = new();

		/// <summary>
		///     How many state changes may occur in a single Evaluate() before execution is interrupted.
		/// </summary>
		/// <remarks>The value is intended to prevent infinite loops.</remarks>
		public Int32 MaxStateChangesPerEvaluate { get; set; } = 100;
		/// <summary>
		///     If true, evaluation continues after a state change occured. If false, a state change will exit Evaluate().
		/// </summary>
		public Boolean AllowMultipleStateChanges { get; set; }
		/// <summary>
		///     Is true if the Statemachine reached a "final state", a state without any transitions.
		/// </summary>
		public Boolean IsStopped => ActiveState.IsFinalState();
		/// <summary>
		///     In addition to the OnStateChange event this can be used to check if a state change occured.
		///     The value persists until the next call to Evaluate().
		/// </summary>
		public Boolean DidChangeState { get; private set; }

		private FSM() {} // forbidden default ctor

		/// <summary>
		///     Creates a named Statemachine instance.
		/// </summary>
		/// <param name="statemachineName">Name or description (required)</param>
		public FSM(String statemachineName) => Name = statemachineName;

		/// <summary>
		///     Provides the states for the statemachine. Must only be called once per statemachine.
		/// </summary>
		/// <param name="states"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public FSM WithStates(params State[] states)
		{
			if (m_States != null)
				throw new InvalidOperationException("States already set!");

			m_States = states;

			return this;
		}

		/// <summary>
		///     Initializes (starts) the statemachine.
		/// </summary>
		/// <returns></returns>
		public FSM Start()
		{
			if (Started)
				throw new InvalidOperationException($"FSM '{Name}': Start() must only be called once");

			ValidateStatemachine();

			m_ActiveStateIndex = 0;

			foreach (var state in m_States)
				state.OnStart(this);

			ActiveState.OnEnterState(this);

			return this;
		}

		private void Stop()
		{
			ActiveState.OnExitState(this);

			foreach (var state in m_States)
				state.OnStop(this);
		}

		/// <summary>
		///     Evaluates the statemachine's states and their transitions.
		/// </summary>
		public void Update()
		{
			ThrowIfStatemachineNotStarted();

			if (IsStopped)
				return;

			var iterations = AllowMultipleStateChanges ? MaxStateChangesPerEvaluate : 0;
			do
			{
				DidChangeState = false;

				var updatingState = ActiveState;
				updatingState.Update(this);

				if (DidChangeState)
				{
					updatingState.OnExitState(this);
					ActiveState.OnEnterState(this);

					OnStateChange?.Invoke(new StateChangeEventArgs
						{ Statemachine = this, PreviousState = updatingState, ActiveState = ActiveState });

					if (ActiveState.IsFinalState())
					{
						Stop();
						OnStopped?.Invoke(new StatemachineStoppedEventArgs { Statemachine = this, FinalState = ActiveState });
					}

					iterations--;
				}

				if (IsStopped || DidChangeState == false)
					break;
			} while (iterations > 0);
		}

		internal void SetActiveState(State state)
		{
			var newStateIndex = FindStateIndex(state);
			DidChangeState = newStateIndex != m_ActiveStateIndex;
			m_ActiveStateIndex = newStateIndex;
		}

		private Int32 FindStateIndex(State searchForState) => Array.FindIndex(m_States, s => s == searchForState);

		/// <summary>
		///     Sent with state change event.
		/// </summary>
		public struct StateChangeEventArgs
		{
			public FSM Statemachine;
			public State PreviousState;
			public State ActiveState;
		}

		/// <summary>
		///     Sent with Statemachine stopped event.
		/// </summary>
		public struct StatemachineStoppedEventArgs
		{
			public FSM Statemachine;
			public State FinalState;
		}
	}
}
