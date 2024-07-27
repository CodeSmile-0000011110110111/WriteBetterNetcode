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
		[Test]
		public void Statemachine_StartEndStates_RunsToEndAndRaisesStateChangeEvent()
		{
			var didExecuteActions = false;

			var sm = new Statemachine("Test FSM", new Statemachine.State[]
			{
				new("START", new Statemachine.Transition[]
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
						"END"),
				}),
				new("END"),
			});

			Assert.AreEqual("START", sm.ActiveState.Name);
			Assert.IsFalse(didExecuteActions);
			sm.Update();

			Assert.AreEqual("END", sm.ActiveState.Name);
			Assert.IsTrue(sm.IsFinished);
			Assert.IsTrue(didExecuteActions);
		}

		[Test]
		public void Statemachine_StartEndStates_InvokesStateChangeEvent()
		{
			var didInvokeStateChangedEvent = false;

			var sm = new Statemachine("Test FSM", new Statemachine.State[]
			{
				new("START", new Statemachine.Transition[]
				{
					new(null,null, "END"),
				}),
				new("END"),
			});

			sm.OnStateChanged += args =>
			{
				didInvokeStateChangedEvent = true;
				Assert.AreEqual(args.PreviousState, sm["START"]);
				Assert.AreEqual(args.ActiveState, sm["END"]);
			};

			sm.Update();

			Assert.IsTrue(didInvokeStateChangedEvent);
		}


		[Test]
		public void Statemachine_NonExistingGotoStateName_Throws()
		{
#if DEBUG || DEVELOPMENT_BUILD
			Assert.Throws<ArgumentException>(() =>
			{
				new Statemachine("Test FSM", new Statemachine.State[]
				{
					new("START", new Statemachine.Transition[]
					{
						new(null, null, "THIS STATE DOES NOT EXIST"),
					}),
					new("END"),
				});
			});
#endif
		}
	}
}
