// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		private void ThrowIfStatemachineNotStarted()
		{
#if DEBUG || DEVELOPMENT_BUILD
			if (m_ActiveStateIndex < 0)
				throw new InvalidOperationException($"FSM '{Name}': Start() not called before Evaluate()!");
#endif
		}

		private void ValidateStatemachine()
		{
#if DEBUG || DEVELOPMENT_BUILD
			if (String.IsNullOrWhiteSpace(Name))
				throw new ArgumentException("FSM has no name");

			if (m_States == null || m_States.Length == 0)
				throw new ArgumentException($"FSM '{Name}': has no states");

			var stateNames = new HashSet<String>();

			for (var stateIndex = 0; stateIndex < m_States.Length; stateIndex++)
			{
				var state = m_States[stateIndex];
				if (state == null)
					throw new ArgumentException($"FSM '{Name}': state at index {stateIndex} is null");
				if (String.IsNullOrWhiteSpace(state.Name))
					throw new ArgumentException($"FSM '{Name}': state at index {stateIndex} has no name");
				if (state.Transitions == null)
					throw new ArgumentException($"FSM '{Name}': '{state.Name}' transitions are null");
				if (stateNames.Contains(state.Name))
					throw new ArgumentException($"FSM '{Name}': state with same name '{state.Name}' already exists!");

				stateNames.Add(state.Name);

				for (var transIndex = 0; transIndex < state.Transitions.Length; transIndex++)
				{
					var transition = state.Transitions[transIndex];
					if (transition == null)
					{
						throw new ArgumentException($"FSM '{Name}': {state.Name} transition '{transition.Name}' at index" +
						                            $" {transIndex} is null");
					}

					var conditions = transition.Conditions;
					if (conditions == null || conditions.Length == 0)
					{
						throw new ArgumentException($"FSM '{Name}': {state.Name} transition '{transition.Name}' at index" +
						                            $" {transIndex} has no conditions, this is not allowed!");
					}

					if (transition.GotoState != null)
					{
						if (transition.GotoState == state)
						{
							throw new ArgumentException($"FSM '{Name}': {state.Name} transition at index {transIndex}" +
							                            " points to the same state. If a self-transition was intentional," +
							                            " simply omit the state parameter.");
						}

						var foundStateIndex = FindStateIndex(transition.GotoState);
						if (foundStateIndex < 0)
						{
							throw new ArgumentException($"FSM '{Name}': {state.Name} transition at index {transIndex}" +
							                            $" points to non-existing state '{transition.GotoState.Name}'");
						}
					}
				}
			}
#endif
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
