// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Actions;
using CodeSmile.Statemachine.Conditions;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		/// <summary>
		///     Creates a new State. 'FSM.S(..)' is shorthand for 'new FSM.State(..)'
		/// </summary>
		/// <param name="stateName"></param>
		/// <returns></returns>
		public static State CreateState(String stateName) => new(stateName);

		/// <summary>
		///     Creates a new unnamed transition. 'FSM.T(..)' is shorthand for 'new FSM.Transition(..)'
		/// </summary>
		/// <returns></returns>
		public static Transition CreateTransition() => new();

		/// <summary>
		///     Creates a named transition to the target state. 'FSM.T(..)' is shorthand for 'new FSM.Transition(..)'
		/// </summary>
		/// <param name="transitionName"></param>
		/// <param name="gotoState"></param>
		/// <returns></returns>
		public static Transition CreateTransition(String transitionName) => new(transitionName);

		/// <summary>
		///     Creates a new generic lambda Condition. 'FSM.C(..)' is shorthand for 'new FSM.Condition(..)'
		/// </summary>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static LambdaCondition Condition(Func<Boolean> callback) => new(callback);

		/// <summary>
		///     Creates a new generic lambda Action. 'FSM.A(..)' is shorthand for 'new FSM.Action(..)'
		/// </summary>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static LambdaAction Action(Action callback) => new(callback);

		/// Logical NOT condition will be true if the containing condition evaluates to false, and vice versa.
		/// </summary>
		/// <param name="condition"></param>
		/// <returns></returns>
		public static LogicalNot NOT(ICondition condition) => new(condition);

		/// <summary>
		///     Logical OR condition will be true if one or more of the containing conditions are true.
		/// </summary>
		/// <param name="conditions">Two or more ICondition instances.</param>
		/// <returns></returns>
		/// <summary>
		public static LogicalOr OR(params ICondition[] conditions) => new(conditions);

		/// <summary>
		///     Logical NOR condition will be true only if both conditions are false.
		/// </summary>
		/// <param name="conditions"></param>
		/// <returns></returns>
		public static LogicalNor NOR(params ICondition[] conditions) => new(conditions);

		/// <summary>
		///     Logical AND condition will be true if all of the containing conditions are true.
		/// </summary>
		/// <remarks>
		///     Logical AND is the default operation for Conditions. This AND operator is intended to be used within
		///     an OR condition to express more complex conditions like so: OR(AND(a,b), AND(c,d), AND(e,f,g,h))
		/// </remarks>
		/// <param name="conditions">Two or more ICondition instances.</param>
		/// <returns></returns>
		public static LogicalAnd AND(params ICondition[] conditions) => new(conditions);

		/// <summary>
		///     Logical NAND condition will be true if one or all of the containing conditions are false.
		/// </summary>
		/// <param name="conditions">Two or more ICondition instances.</param>
		/// <returns></returns>
		public static LogicalNand NAND(params ICondition[] conditions) => new(conditions);
	}
}
