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

		private readonly String m_Name;
		private State[] m_States;

		private Int32 m_ActiveStateIndex;

		private Boolean m_DidChangeState;
		internal Boolean DidChangeState => m_DidChangeState;

		/// <summary>
		///     Global variables are available for all statemachines (be mindful!).
		/// </summary>
		public Variables GlobalVars => s_GlobalVars;

		/// <summary>
		///     Local variables are unique to the current Statemachine instance.
		/// </summary>
		public Variables LocalVars { get; } = new();

		public String Name => m_Name;
		public State ActiveState => m_States[m_ActiveStateIndex];
		public Boolean IsStopped => ActiveState.IsFinalState();

		public State this[String stateName] => m_States[FindStateIndexByName(stateName)];

		public FSM(String statemachineName)
		{
			if (String.IsNullOrWhiteSpace(statemachineName))
				throw new ArgumentException("invalid name", nameof(statemachineName));

			m_Name = statemachineName;
		}

		public FSM SetStates(State[] states)
		{
			if (m_States != null)
				throw new InvalidOperationException("States already initialized!");
			if (states == null || states.Length == 0)
				throw new ArgumentException("no states provided", nameof(states));

			m_States = states;
			ValidateStateNames();

			return this;
		}

		public FSM StartWithStates(State[] states)
		{
			SetStates(states);
			Start();

			return this;
		}

		public void Start()
		{
			m_ActiveStateIndex = 0;

			foreach (var state in m_States)
				state.OnStart(this);

			ActiveState.OnEnterState(this);
		}

		private void Stop()
		{
			ActiveState.OnExitState(this);

			foreach (var state in m_States)
				state.OnStop(this);
		}

		public void Update()
		{
			if (IsStopped)
			{
				Debug.LogWarning($"called Update() on stopped Statemachine: {m_Name}");
				return;
			}

			var updatingState = ActiveState;
			updatingState.Update(this);

			if (m_DidChangeState)
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

				m_DidChangeState = false;
			}
		}

		internal void SetActiveState(String stateName)
		{
			ThrowIfAlreadyChangedState();

			var newStateIndex = FindStateIndexByName(stateName);
			m_DidChangeState = newStateIndex != m_ActiveStateIndex;
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
			if (m_DidChangeState)
				throw new InvalidOperationException("state change already occured this update!");
		}

		private void WarnIfStateDidNotChange(String stateName)
		{
			if (m_DidChangeState == false)
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

		internal enum VariableScope
		{
			Local,
			Global,
		}

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

#if UNITY_EDITOR
		[InitializeOnLoadMethod] private static void ResetStaticFields() =>
			EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;

		private static void OnPlaymodeStateChanged(PlayModeStateChange playModeState)
		{
			if (playModeState == PlayModeStateChange.ExitingPlayMode)
				s_GlobalVars.Clear();
		}
#endif
	}
}
