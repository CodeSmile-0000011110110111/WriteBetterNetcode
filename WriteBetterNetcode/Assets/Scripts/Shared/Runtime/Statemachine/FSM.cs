﻿// Copyright (C) 2021-2024 Steffen Itterheim
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

		private static readonly Variables s_StaticVars = new();

		private State[] m_States;

		private Int32 m_ActiveStateIndex = -1;

		private Boolean m_Logging;
		public Boolean Logging
		{
			get => m_Logging;
			set
			{
				m_Logging = value;
				ApplyLoggingToAllStates(m_Logging);
			}
		}

		public State[] States => m_States;
		private Boolean IsStarted => !(m_ActiveStateIndex < 0);

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
				return States[m_ActiveStateIndex];
			}
		}

		/// <summary>
		///     Global variables are available for all statemachines (be mindful!).
		/// </summary>
		public Variables StaticVars => s_StaticVars;

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

		private void ApplyLoggingToAllStates(Boolean logging)
		{
			if (m_States == null || m_States.Length == 0)
				return;

			foreach (var state in m_States)
			{
				if (state != null)
					state.Logging = logging;
			}
		}

		/// <summary>
		///     Creates several named States. For use with Enum.GetNames().
		/// </summary>
		/// <remarks>Ideal for use with Enum.GetNames().</remarks>
		/// <param name="stateNames"></param>
		/// <returns></returns>
		public FSM WithStates(params String[] stateNames)
		{
			if (stateNames == null)
				throw new ArgumentNullException(nameof(stateNames));

			var stateCount = stateNames.Length;
			var states = new State[stateCount];
			for (var i = 0; i < stateCount; i++)
				states[i] = new State(stateNames[i]);

			WithStates(states);

			return this;
		}

		/// <summary>
		///     Provides the states for the statemachine. Must only be called once per statemachine.
		/// </summary>
		/// <param name="states"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public FSM WithStates(params State[] states)
		{
			if (States != null)
				throw new InvalidOperationException("States already set!");
			if (states == null)
				throw new ArgumentNullException(nameof(states));

			m_States = states;

			return this;
		}

		/// <summary>
		///     Initializes (starts) the statemachine.
		/// </summary>
		/// <returns></returns>
		public FSM Start()
		{
			if (IsStarted)
				throw new InvalidOperationException($"FSM '{Name}': Start() must only be called once");

			m_ActiveStateIndex = 0;

			ApplyLoggingToAllStates(Logging);

			foreach (var state in States)
				state.OnStart(this);

			ValidateStatemachine();

			ActiveState.OnEnterState(this);

			return this;
		}

		private void Stop()
		{
			ActiveState.OnExitState(this);

			foreach (var state in States)
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
				//Debug.Log($"[{Time.frameCount}] FSM Update: iteration #{MaxStateChangesPerEvaluate-iterations}");

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

		private Int32 FindStateIndex(State searchForState) => Array.FindIndex(States, s => s == searchForState);

		public override String ToString() => $"FSM({Name})";

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
