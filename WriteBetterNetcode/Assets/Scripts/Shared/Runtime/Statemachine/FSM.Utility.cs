// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Conditions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
#if UNITY_EDITOR
		[InitializeOnLoadMethod] private static void ResetGlobalVars() => EditorApplication.playModeStateChanged += state =>
		{
			if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingEditMode)
				s_StaticVars.Clear();
		};
#endif
		public override String ToString() => $"FSM({Name})";

		public String ToPlantUml()
		{
			if (!IsStarted)
				throw new Exception($"FSM '{Name}': can only generate PlantUML after statemachine started");

			var statesBuilder = new StringBuilder();
			var transBuilder = new StringBuilder();

			for (var stateIndex = 0; stateIndex < States.Length; stateIndex++)
			{
				var state = States[stateIndex];
				var stateId = $"state{stateIndex}";

				statesBuilder.AppendLine($"state \"{state.Name}\" as {stateId}");
				statesBuilder.AppendLine($"state {stateId} {{");

				if (stateIndex == 0)
					transBuilder.AppendLine($"[*] -> {stateId}");

				for (var transIndex = 0; transIndex < state.Transitions.Length; transIndex++)
				{
					var trans = state.Transitions[transIndex];
					var transId = $"trans{transIndex}";
					var transStateId = $"{stateId}_{transId}";

					var transName = trans.Name;
					if (transName == null)
					{
						transName = trans.GotoState != null ? trans.GotoState.Name :
							trans.Conditions.Length == 1 ? trans.Conditions[0].ToDebugString(this) : " ";
					}

					statesBuilder.AppendLine($"\tstate \"{transName}\" as {transStateId}");
					statesBuilder.AppendLine($"\tstate {transStateId} #line.dotted {{");

					for (var condIndex = 0; condIndex < trans.Conditions.Length; condIndex++)
					{
						var cond = trans.Conditions[condIndex];
						var satisfied = cond.IsSatisfied(this);

						var negated = false;
						if (cond is LogicalNot notCondition)
							cond = notCondition;
						else if (cond is LogicalOr orCondition)
							cond = orCondition;
						else if (cond is LogicalAnd andCondition)
							cond = andCondition;

						statesBuilder.AppendLine(
							$"\t\t{stateId}_{transId} : {cond.ToDebugString(this)} | \"\"{satisfied}\"\"");
					}

					statesBuilder.AppendLine($"\t\t{stateId}_{transId} : ....");

					for (var actIndex = 0; actIndex < trans.Actions.Length; actIndex++)
					{
						var act = trans.Actions[actIndex];
						statesBuilder.AppendLine($"\t\t{stateId}_{transId} : {act.ToDebugString(this)}");
					}

					statesBuilder.AppendLine("\t}");

					if (trans.GotoState != null)
					{
						var gotoStateId = $"state{FindStateIndex(trans.GotoState)}";
						transBuilder.AppendLine($"{stateId} --> {gotoStateId} : {transName}");
						//transBuilder.AppendLine($"{transStateId} --> {gotoStateId} : {transName}");
					}
				}

				statesBuilder.AppendLine("}");
			}

			statesBuilder.AppendLine();

			return $"title {nameof(FSM)}: {Name}\n\n{statesBuilder}\n{transBuilder}";
		}

		private void ThrowIfStatemachineNotStarted()
		{
#if DEBUG || DEVELOPMENT_BUILD
			if (!IsStarted)
				throw new InvalidOperationException($"FSM '{Name}': Start() not called before Evaluate()!");
#endif
		}

		private void ValidateStatemachine()
		{
#if DEBUG || DEVELOPMENT_BUILD
			if (String.IsNullOrWhiteSpace(Name))
				throw new ArgumentException("FSM has no name");

			if (States == null || States.Length == 0)
				throw new ArgumentException($"FSM '{Name}': has no states");

			var stateNames = new HashSet<String>();
			var statesUsed = new HashSet<String>();

			for (var stateIndex = 0; stateIndex < States.Length; stateIndex++)
			{
				var state = States[stateIndex];
				if (state == null)
					throw new ArgumentException($"FSM '{Name}': state at index {stateIndex} is null");
				if (String.IsNullOrWhiteSpace(state.Name))
					throw new ArgumentException($"FSM '{Name}': state at index {stateIndex} has no name");
				if (state.Transitions == null)
					throw new ArgumentException($"FSM '{Name}': '{state.Name}' transitions are null");
				if (stateNames.Contains(state.Name))
					throw new ArgumentException($"FSM '{Name}': state with same name '{state.Name}' already exists!");

				stateNames.Add(state.Name);

				// first state is always used (start state)
				if (stateIndex == 0)
					statesUsed.Add(state.Name);

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
						Debug.LogWarning($"FSM '{Name}': {state.Name} transition '{transition.Name}' at index" +
						                 $" {transIndex} has no conditions, transition will be taken instantly!");
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

						statesUsed.Add(transition.GotoState.Name);
					}
				}
			}

			if (stateNames.Count != statesUsed.Count)
			{
				var first = true;
				var sb = new StringBuilder($"FSM '{Name}': States w/o transitions leading to them: ");
				foreach (var stateName in stateNames)
				{
					if (statesUsed.Contains(stateName) == false)
					{
						if (!first)
							sb.Append(", ");
						first = false;

						sb.Append(stateName);
					}
				}

				Debug.LogWarning(sb.ToString());
			}
#endif
		}

		internal String GetDebugVarName(VariableBase variable)
		{
			var isGlobal = false;
			var varName = Vars.FindVariableName(variable);
			if (varName == null)
			{
				varName = StaticVars.FindVariableName(variable);
				isGlobal = true;
			}

			if (varName == null)
				return variable.ToString();

			var scope = isGlobal ? "s" : "m";
			return $"{scope}_{varName}";
		}
	}
}
