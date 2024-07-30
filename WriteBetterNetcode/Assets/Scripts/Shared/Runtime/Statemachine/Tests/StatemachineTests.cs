// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Tests
{
	public class StatemachineTests
	{
		[Test]
		public void SM_StartEndStates_RunsToEndAndRaisesStateChangeEvent()
		{
			var didExecuteActions = false;

			var startState = "START";
			var endState = "END";
			var sm = new FSM("Test FSM").StartWithStates(new FSM.State[]
			{
				new(startState, new FSM.Transition[]
				{
					new(new FSM.ICondition[]
						{
							new FSM.Condition(() => true),
							new FSM.Condition(() => true),
						},
						new FSM.IAction[]
						{
							new FSM.Action(() => didExecuteActions = true),
						},
						endState),
				}),
				new(endState),
			});

			Assert.AreEqual(startState, sm.ActiveState.Name);
			Assert.IsFalse(didExecuteActions);
			sm.Update();

			Assert.AreEqual(endState, sm.ActiveState.Name);
			Assert.IsTrue(sm.IsStopped);
			Assert.IsTrue(didExecuteActions);
		}

		[Test]
		public void SM_StateChange_InvokesStateChangeEvent()
		{
			var didInvokeStateChangedEvent = false;

			var startState = "START";
			var endState = "END";
			var sm = new FSM("Test FSM").StartWithStates(new FSM.State[]
			{
				new(startState, new FSM.Transition[]
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
				new FSM("Test FSM").StartWithStates(new FSM.State[]
				{
					new(startState, new FSM.Transition[]
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

			var sm = new FSM("Test FSM");
			var testVar1 = sm.LocalVars.DefineInt("TestVar1", expectedValue);
			var didExecute = sm.LocalVars.DefineBool("didExecute");

			sm.StartWithStates(new FSM.State[]
			{
				new(startState, new FSM.Transition[]
				{
					new(new FSM.ICondition[]
						{
							new FSM.Variable.IsEqual(testVar1, expectedValue),
						},
						new FSM.IAction[]
						{
							new FSM.Variable.SetTrue(didExecute),
						},
						endState),
				}),
				new(endState),
			});

			sm.Update();

			Assert.IsTrue(didExecute.BoolValue);
		}

		[Test]
		public void SMVar_VarCondition_SetsExpectedValue()
		{
			var startState = "START";
			var endState = "END";
			var expectedValue = 12345;

			var sm = new FSM("Test FSM");
			var testVar1 = sm.LocalVars.DefineBool("TestVar1", true);
			var testVar2 = sm.LocalVars.DefineInt("TestVar2");
			var didExecute = sm.LocalVars.DefineBool("did execute");

			sm.StartWithStates(new FSM.State[]
			{
				new(startState, new FSM.Transition[]
				{
					new(new FSM.ICondition[]
						{
							new FSM.Variable.IsTrue(testVar1),
						},
						new FSM.IAction[]
						{
							new FSM.Variable.SetInt(testVar2, expectedValue),
							new FSM.Variable.SetTrue(didExecute),
						},
						endState),
				}),
				new(endState),
			});

			sm.Update();

			Assert.AreEqual(expectedValue, testVar2.IntValue);
			Assert.IsTrue(didExecute.BoolValue);
		}
	}
}
