// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.FSM.Tests
{
	public class StatemachineTests
	{
		private static readonly String TestVar1 = "Test1";
		private static readonly String TestVar2 = "Test2";

		[Test]
		public void SM_StartEndStates_RunsToEndAndRaisesStateChangeEvent()
		{
			var didExecuteActions = false;

			var startState = "START";
			var endState = "END";
			var sm = new Statemachine("Test FSM").InitStates(new Statemachine.State[]
			{
				new(startState, new Statemachine.Transition[]
				{
					new(new Statemachine.ICondition[]
						{
							new Statemachine.Condition(() => true),
							new Statemachine.Condition(() => true),
						},
						new Statemachine.IAction[]
						{
							new Statemachine.Action(() => didExecuteActions = true),
						},
						endState),
				}),
				new(endState),
			});

			Assert.AreEqual(startState, sm.ActiveState.Name);
			Assert.IsFalse(didExecuteActions);
			sm.Update();

			Assert.AreEqual(endState, sm.ActiveState.Name);
			Assert.IsTrue(sm.IsFinished);
			Assert.IsTrue(didExecuteActions);
		}

		[Test]
		public void SM_StateChange_InvokesStateChangeEvent()
		{
			var didInvokeStateChangedEvent = false;

			var startState = "START";
			var endState = "END";
			var sm = new Statemachine("Test FSM").InitStates(new Statemachine.State[]
			{
				new(startState, new Statemachine.Transition[]
				{
					new(null, null, endState),
				}),
				new(endState),
			});

			sm.OnStateChanged += args =>
			{
				didInvokeStateChangedEvent = true;
				Assert.AreEqual(args.PreviousState, sm[startState]);
				Assert.AreEqual(args.ActiveState, sm[endState]);
			};

			sm.Update();

			Assert.IsTrue(didInvokeStateChangedEvent);
		}

		[Test]
		public void SM_NonExistingGotoStateName_Throws()
		{
#if DEBUG || DEVELOPMENT_BUILD
			Assert.Throws<ArgumentException>(() =>
			{
				var startState = "START";
				var endState = "END";
				new Statemachine("Test FSM").InitStates(new Statemachine.State[]
				{
					new(startState, new Statemachine.Transition[]
					{
						new(null, null, "THIS STATE DOES NOT EXIST"),
					}),
					new(endState),
				});
			});
#endif
		}

		[Test]
		public void SMVar_LocalIntEquals_IsTrue()
		{
			var startState = "START";
			var endState = "END";
			var expectedValue = -111111;
			var didExecute = "didExecute";

			var sm = new Statemachine("Test FSM");
			sm.LocalVars.DefineInt(TestVar1, expectedValue);

			sm.InitStates(new Statemachine.State[]
			{
				new(startState, new Statemachine.Transition[]
				{
					new(new Statemachine.ICondition[]
						{
							new IsEqual(TestVar1, expectedValue),
						},
						new Statemachine.IAction[]
						{
							new Statemachine.Action(() =>
							{
								sm.LocalVars[didExecute].BoolValue = true;
							}),
						},
						endState),
				}),
				new(endState),
			});

			sm.Update();

			Assert.IsTrue(sm.LocalVars[didExecute].BoolValue);
		}

		[Test]
		public void SMVar_VarCondition_SetsExpectedValue()
		{
			var startState = "START";
			var endState = "END";
			var expectedValue = 12345;

			var sm = new Statemachine("Test FSM");
			sm.LocalVars.DefineBool(TestVar1, true);

			sm.InitStates(new Statemachine.State[]
			{
				new(startState, new Statemachine.Transition[]
				{
					new(new Statemachine.ICondition[]
						{
							new Statemachine.Condition(() =>
							{
								return sm.LocalVars[TestVar1].BoolValue;
							}),
						},
						new Statemachine.IAction[]
						{
							new Statemachine.Action(() =>
							{
								sm.LocalVars[TestVar2].IntValue = expectedValue;
							}),
						},
						endState),
				}),
				new(endState),
			});

			sm.Update();

			Assert.AreEqual(expectedValue, sm.LocalVars[TestVar2].IntValue);
		}
	}
}
