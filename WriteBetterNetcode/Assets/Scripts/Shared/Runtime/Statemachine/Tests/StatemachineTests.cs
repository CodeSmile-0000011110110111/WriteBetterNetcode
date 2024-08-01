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
		[TestCase(null)] [TestCase("")]
		public void FSM_InvalidStatemachineName_Throws(String fsmName)
		{
#if DEBUG || DEVELOPMENT_BUILD
			Assert.Throws<ArgumentException>(() => new FSM(fsmName).WithStates(FSM.S("X")).Start());
#endif
		}

		[TestCase(null)] [TestCase("")]
		public void FSM_InvalidStateName_Throws(String stateName)
		{
#if DEBUG || DEVELOPMENT_BUILD
			Assert.Throws<ArgumentException>(() => new FSM("FSM").WithStates(FSM.S(stateName)).Start());
#endif
		}

		[Test]
		public void FSM_StartNotCalledBeforeEvaluate_Throws()
		{
#if DEBUG || DEVELOPMENT_BUILD
			Assert.Throws<InvalidOperationException>(() => new FSM("FSM").WithStates(FSM.S("X")).Evaluate());
#endif
		}

		[Test]
		public void FSM_NonExistingGotoState_Throws()
		{
#if DEBUG || DEVELOPMENT_BUILD
			Assert.Throws<ArgumentException>(() =>
				new FSM("FSM").WithStates(FSM.S("X").WithTransitions(FSM.T(FSM.S("?")))).Start());
#endif
		}

		[Test]
		public void FSM_StartEndStates_RunsToEndAndRaisesStateChangeEvent()
		{
			var didExecuteActions = false;
			var endState = FSM.S("END");
			var startState = FSM.S("START")
				.WithTransitions(FSM.T(endState)
					.WithActions(FSM.A(() => didExecuteActions = true)));
			var sm = new FSM("FSM").WithStates(startState, endState);

			sm.Start().Evaluate();

			Assert.AreEqual(endState, sm.ActiveState);
			Assert.IsTrue(sm.IsStopped);
			Assert.IsTrue(didExecuteActions);
		}

		[Test]
		public void FSM_StateChange_InvokesStateChangeEvent()
		{
			var startState = FSM.S("START");
			var endState = FSM.S("END");
			var sm = new FSM("FSM").WithStates(startState, endState);
			startState.WithTransitions(FSM.T(endState));

			var didInvokeStateChangedEvent = false;
			sm.OnStateChange += args =>
			{
				didInvokeStateChangedEvent = true;
				Assert.AreEqual(args.PreviousState, startState);
				Assert.AreEqual(args.ActiveState, endState);
			};

			sm.Start().Evaluate();

			Assert.IsTrue(didInvokeStateChangedEvent);
		}

		[TestCase(false, false, false)]
		[TestCase(true, false, true)]
		[TestCase(false, true, true)]
		[TestCase(true, true, true)]
		public void FSM_LogicalOrCondition_ExpectedOutcome(Boolean condition1, Boolean condition2, Boolean expected)
		{
			var actual = false;
			var startState = FSM.S("START")
				.WithTransitions(FSM.T()
					.WithConditions(FSM.OR(FSM.C(() => condition1), FSM.C(() => condition2)))
					.WithActions(FSM.A(() => actual = true)));

			new FSM("Test FSM").WithStates(startState).Start().Evaluate();

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void FSMVar_LocalIntEquals_IsTrue()
		{
			var expectedValue = -111111;

			var startState = FSM.S("START");
			var endState = FSM.S("END");
			var sm = new FSM("FSM").WithStates(startState, endState);
			var testVar = sm.LocalVars.DefineInt("TestVar", expectedValue);
			var didExecute = sm.LocalVars.DefineBool("didExecute");

			startState.WithTransitions(FSM.T(endState)
				.WithConditions(FSM.Variable.IsEqual(testVar, expectedValue))
				.WithActions(FSM.Variable.SetTrue(didExecute)));

			sm.Start().Evaluate();

			Assert.IsTrue(didExecute.BoolValue);
		}

		[Test]
		public void FSMVar_VarCondition_SetsExpectedValue()
		{
			var startState = FSM.S("START");
			var endState = FSM.S("END");
			var sm = new FSM("FSM").WithStates(startState, endState);
			var testVar1 = sm.LocalVars.DefineBool("TestVar1", true);
			var testVar2 = sm.LocalVars.DefineInt("TestVar2");
			var didExecute = sm.LocalVars.DefineBool("did execute");

			var expectedValue = 12345;
			startState.WithTransitions(FSM.T(endState)
				.WithConditions(FSM.Variable.IsTrue(testVar1))
				.WithActions(FSM.Variable.SetInt(testVar2, expectedValue), FSM.Variable.SetTrue(didExecute)));

			sm.Start().Evaluate();

			Assert.AreEqual(expectedValue, testVar2.IntValue);
			Assert.IsTrue(didExecute.BoolValue);
		}
	}
}
