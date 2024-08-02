// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		/// <summary>
		///     Event methods that the Statemachine calls at specific times.
		/// </summary>
		public interface IStatemachineEvents
		{
			/// <summary>
			///     Called before the first evaluation of the Statemachine. Similar to Start() in a MonoBehaviour.
			/// </summary>
			/// <remarks>Use this for once-only initialization.</remarks>
			/// <param name="sm"></param>
			void OnStart(FSM sm) {}

			/// <summary>
			///     Called when the Statemachine stops, meaning it reached a State that has no transitions.
			/// </summary>
			/// <remarks>Use this for final cleanup of the Action/Condition.</remarks>
			/// <remarks>
			///     Before OnStop the OnExitState event is raised even though there is no state change, and likely
			///     no Actions/Conditions anyway. But in case where a Statemachine is manually stopped this would ensure pairing
			///     of OnEnterState and OnExitState.
			/// </remarks>
			/// <param name="sm"></param>
			void OnStop(FSM sm) {}

			/// <summary>
			///     Called when the Statemachine enters a state to which the Action/Condition belongs.
			/// </summary>
			/// <remarks>Use this to set up the Action/Condition, such as registering an event.</remarks>
			/// <param name="sm"></param>
			void OnEnterState(FSM sm) {}

			/// <summary>
			///     Called when the Statemachine leaves the state to which the Action/Condition belongs.
			/// </summary>
			/// <remarks>Use this to deactivate the Action/Condition, eg unregister an event.</remarks>
			/// <remarks>
			///     Before a Statemachine stops it will also issue an OnExitState event to guarantee the pairing of
			///     Enter/Exit events, primarily in cases where the Statemachine is stopped manually.
			/// </remarks>
			/// <param name="sm"></param>
			void OnExitState(FSM sm) {}
		}
	}
}
