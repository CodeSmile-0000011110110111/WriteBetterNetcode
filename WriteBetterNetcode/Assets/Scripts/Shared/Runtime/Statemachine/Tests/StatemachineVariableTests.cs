// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Conditions;
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Tests
{
	public class StatemachineVariableTests
	{
		private const String TestVar1 = "TestVar1";

		[SetUp]
		public void SetUp() => new FSM("FSM").StaticVars.Clear();

		[TestCase(false)] [TestCase(true)]
		public void FSMVar_DefineBool_ReturnsExpectedValue(Boolean value)
		{
			var sm = new FSM("FSM");
			sm.Vars.DefineBool(TestVar1, value);
			sm.StaticVars.DefineBool(TestVar1, value);

			Assert.AreEqual(value, sm.Vars.GetBool(TestVar1).Value);
			Assert.AreEqual(value, sm.StaticVars.GetBool(TestVar1).Value);
		}

		[TestCase(true)] [TestCase(false)]
		public void VarCondition_IsTrue_ChangesState(Boolean value)
		{
			var sm = new FSM("FSM").WithStates("START", "END");
			var boolVar = sm.Vars.DefineBool(TestVar1, value);

			sm.States[0].AddTransition().To(sm.States[1]).WithConditions(new IsTrue(boolVar));
			sm.Start().Update();

			Assert.AreEqual(sm.ActiveState, value ? sm.States[1] : sm.States[0]);
		}

		[TestCase(true)] [TestCase(false)]
		public void VarCondition_IsFalse_ChangesState(Boolean value)
		{
			var sm = new FSM("FSM").WithStates("START", "END");
			var boolVar = sm.Vars.DefineBool(TestVar1, value);

			sm.States[0].AddTransition().To(sm.States[1]).WithConditions(new IsFalse(boolVar));
			sm.Start().Update();

			Assert.AreEqual(sm.ActiveState, value ? sm.States[0] : sm.States[1]);
		}
	}
}
