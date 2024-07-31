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
		public event Action<StateChangeEventArgs> OnStateChanged;

		/// <summary>
		///     Invoked when the statemachine entered a final state (state without any transitions).
		/// </summary>
		public event Action<StatemachineStoppedEventArgs> OnStopped;

		private static readonly Variables s_GlobalVars = new();

		private State[] m_States;

		private Int32 m_ActiveStateIndex = -1;

		public String Name { get; }
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
		///     Local variables are unique to the current Statemachine instance.
		/// </summary>
		public Variables LocalVars { get; } = new();

		public Int32 MaxStateChangesPerEvaluate { get; set; } = 100;
		public Boolean AllowMultipleStateChanges { get; set; }
		public Boolean IsStopped => ActiveState.IsFinalState();
		public Boolean DidChangeState { get; private set; }

		private FSM() {} // forbidden default ctor

		public FSM(String statemachineName) => Name = statemachineName;

		public State[] WithStateNames(params String[] stateNames)
		{
			if (m_States != null)
				throw new InvalidOperationException("States already set!");
			if (stateNames == null || stateNames.Length == 0)
				throw new ArgumentException("No state names provided");

			m_States = new State[stateNames.Length];
			for (var i = 0; i < stateNames.Length; i++)
				m_States[i] = new State(stateNames[i]);

			return m_States;
		}

		public FSM WithStates(params State[] states)
		{
			if (m_States != null)
				throw new InvalidOperationException("States already set!");

			m_States = states;

			return this;
		}

		/// <summary>
		///     Initializes the statemachine.
		/// </summary>
		/// <returns></returns>
		public FSM Start()
		{
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
		///     Evaluates the current state's transitions.
		/// </summary>
		public void Evaluate()
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

					OnStateChanged?.Invoke(new StateChangeEventArgs
						{ Fsm = this, PreviousState = updatingState, ActiveState = ActiveState });

					if (ActiveState.IsFinalState())
					{
						Stop();
						OnStopped?.Invoke(new StatemachineStoppedEventArgs { Fsm = this, FinalState = ActiveState });
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

		public struct StateChangeEventArgs
		{
			public FSM Fsm;
			public State PreviousState;
			public State ActiveState;
		}

		public struct StatemachineStoppedEventArgs
		{
			public FSM Fsm;
			public State FinalState;
		}
	}
}
