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
		public void FSM_StartNotCalledBeforeEvaluate_Throws()
		{
#if DEBUG || DEVELOPMENT_BUILD
			Assert.Throws<InvalidOperationException>(() =>
			{
				var startState = new FSM.State("START");
				var sm = new FSM("FSM").WithStates(startState);
				var testVar = sm.LocalVars.DefineBool("TestVar", true);
				startState.WithTransitions(new FSM.Transition[]
				{
					new(new FSM.ICondition[] { FSM.Variable.IsTrue(testVar) }, new FSM.State("MISS")),
				});

				sm.Evaluate();
			});
#endif
		}

		[TestCase(null), TestCase("")]
		public void FSM_NoStatemachineName_Throws(string fsmName)
		{
#if DEBUG || DEVELOPMENT_BUILD
			Assert.Throws<ArgumentException>(() =>
			{
				var startState = new FSM.State("START");
				var sm = new FSM(fsmName).WithStates(startState);
				var testVar = sm.LocalVars.DefineBool("TestVar", true);
				startState.WithTransitions(new FSM.Transition[]
				{
					new(new FSM.ICondition[] { FSM.Variable.IsTrue(testVar) }, new FSM.State("MISS")),
				});

				sm.Start();
			});
#endif
		}

		[TestCase(null), TestCase("")]
		public void FSM_NoStateName_Throws(string stateName)
		{
#if DEBUG || DEVELOPMENT_BUILD
			Assert.Throws<ArgumentException>(() =>
			{
				var startState = new FSM.State(stateName);
				var endState = new FSM.State("END");
				var sm = new FSM("FSM").WithStates(startState, endState);
				var testVar = sm.LocalVars.DefineBool("TestVar", true);
				startState.WithTransitions(new FSM.Transition[]
				{
					new(new FSM.ICondition[] { FSM.Variable.IsTrue(testVar) }, endState),
				});

				sm.Start();
			});
#endif
		}

		[Test]
		public void FSM_NonExistingGotoState_Throws()
		{
#if DEBUG || DEVELOPMENT_BUILD
			Assert.Throws<ArgumentException>(() =>
			{
				var startState = new FSM.State("START");
				var sm = new FSM("Test FSM").WithStates(startState);
				var testVar = sm.LocalVars.DefineBool("TestVar", true);
				startState.WithTransitions(new FSM.Transition[]
				{
					new(new FSM.ICondition[] { FSM.Variable.IsTrue(testVar) }, null, new FSM.State("MISS")),
				});

				sm.Start();
			});
#endif
		}

		[Test]
		public void FSM_StartEndStates_RunsToEndAndRaisesStateChangeEvent()
		{
			var didExecuteActions = false;

			var startState = new FSM.State("START");
			var endState = new FSM.State("END");
			var sm = new FSM("Test FSM").WithStates(startState, endState);

			startState.WithTransitions(new FSM.Transition[]
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
			});

			Assert.IsFalse(didExecuteActions);
			sm.Start().Evaluate();

			Assert.AreEqual(endState, sm.ActiveState);
			Assert.IsTrue(sm.IsStopped);
			Assert.IsTrue(didExecuteActions);
		}

		[Test]
		public void FSM_StateChange_InvokesStateChangeEvent()
		{
			var didInvokeStateChangedEvent = false;

			var startState = new FSM.State("START");
			var endState = new FSM.State("END");
			var sm = new FSM("Test FSM").WithStates(startState, endState);
			var testVar = sm.LocalVars.DefineBool("TestVar", true);

			startState.WithTransitions(new FSM.Transition[]
			{
				new(new FSM.ICondition[]
				{
					FSM.Variable.IsTrue(testVar),
				}, null, endState),
			});

			sm.OnStateChanged += args =>
			{
				didInvokeStateChangedEvent = true;
				Assert.AreEqual(args.PreviousState, startState);
				Assert.AreEqual(args.ActiveState, endState);
			};

			sm.Start().Evaluate();

			Assert.IsTrue(didInvokeStateChangedEvent);
		}

		[Test]
		public void FSMVar_LocalIntEquals_IsTrue()
		{
			var expectedValue = -111111;

			var startState = new FSM.State("START");
			var endState = new FSM.State("END");
			var sm = new FSM("Test FSM").WithStates(startState, endState);

			var testVar1 = sm.LocalVars.DefineInt("TestVar1", expectedValue);
			var didExecute = sm.LocalVars.DefineBool("didExecute");

			startState.WithTransitions(new FSM.Transition[]
			{
				new(new FSM.ICondition[]
					{
						FSM.Variable.IsEqual(testVar1, expectedValue),
					},
					new FSM.IAction[]
					{
						FSM.Variable.SetTrue(didExecute),
					},
					endState),
			});

			sm.Start().Evaluate();

			Assert.IsTrue(didExecute.BoolValue);
		}

		[Test]
		public void FSMVar_VarCondition_SetsExpectedValue()
		{
			var expectedValue = 12345;

			var startState = new FSM.State("START");
			var endState = new FSM.State("END");
			var sm = new FSM("Test FSM").WithStates(startState, endState);

			var testVar1 = sm.LocalVars.DefineBool("TestVar1", true);
			var testVar2 = sm.LocalVars.DefineInt("TestVar2");
			var didExecute = sm.LocalVars.DefineBool("did execute");

			startState.WithTransitions(new FSM.Transition[]
			{
				new(new FSM.ICondition[]
					{
						FSM.Variable.IsTrue(testVar1),
					},
					new FSM.IAction[]
					{
						FSM.Variable.SetInt(testVar2, expectedValue),
						FSM.Variable.SetTrue(didExecute),
					},
					endState),
			});

			sm.Start().Evaluate();

			Assert.AreEqual(expectedValue, testVar2.IntValue);
			Assert.IsTrue(didExecute.BoolValue);
		}
	}
}
