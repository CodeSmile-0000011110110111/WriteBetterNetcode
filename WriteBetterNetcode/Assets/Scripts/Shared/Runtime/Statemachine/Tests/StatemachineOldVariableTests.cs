// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Tests
{
	public class StatemachineOldVariableTests
	{
		private static readonly String TestVar1 = "Test1";
		private static readonly String TestVar2 = "Test2";

		[Test]
		public void FSMVars_UndefinedVariables_ReturnDefaultValues()
		{
			var sm = new FSM("FSM");
			sm.OldGlobalVars.Clear();
			Assert.AreEqual(false, sm.OldGlobalVars["undefined-bool"].BoolValue);
			Assert.AreEqual(0f, sm.OldGlobalVars["undefined-float"].FloatValue);
			Assert.AreEqual(0, sm.OldGlobalVars["undefined-int"].IntValue);

			Assert.AreEqual(false, sm.OldVars["undefined-bool"].BoolValue);
			Assert.AreEqual(0f, sm.OldVars["undefined-float"].FloatValue);
			Assert.AreEqual(0, sm.OldVars["undefined-int"].IntValue);
		}

		[Test]
		public void FSMVars_UndefinedVariables_CanBeAssignedTo()
		{
			var sm = new FSM("FSM");
			sm.OldGlobalVars.Clear();
			sm.OldGlobalVars["undefined-bool"].BoolValue = true;
			sm.OldGlobalVars["undefined-float"].FloatValue = 1.2345f;
			sm.OldGlobalVars["undefined-int"].IntValue = -123456;
			Assert.AreEqual(true, sm.OldGlobalVars["undefined-bool"].BoolValue);
			Assert.AreEqual(1.2345f, sm.OldGlobalVars["undefined-float"].FloatValue);
			Assert.AreEqual(-123456, sm.OldGlobalVars["undefined-int"].IntValue);

			sm.OldVars["undefined-bool"].BoolValue = true;
			sm.OldVars["undefined-float"].FloatValue = -1.2345f;
			sm.OldVars["undefined-int"].IntValue = 123456;
			Assert.AreEqual(true, sm.OldVars["undefined-bool"].BoolValue);
			Assert.AreEqual(-1.2345f, sm.OldVars["undefined-float"].FloatValue);
			Assert.AreEqual(123456, sm.OldVars["undefined-int"].IntValue);
		}

		[TestCase(false)] [TestCase(true)]
		public void FSMLocalVar_DefineBool_ReturnsExpectedValue(Boolean value)
		{
			var sm = new FSM("FSM");
			sm.OldVars.DefineBool(TestVar1, value);

			Assert.AreEqual(value, sm.OldVars[TestVar1].BoolValue);
		}

		[TestCase(0f)] [TestCase(Single.MinValue)] [TestCase(Single.MaxValue)]
		public void FSMLocalVar_DefineFloat_ReturnsExpectedValue(Single value)
		{
			var sm = new FSM("FSM");
			sm.OldVars.DefineFloat(TestVar1, value);

			Assert.AreEqual(value, sm.OldVars[TestVar1].FloatValue);
		}

		[TestCase(0)] [TestCase(Int32.MinValue)] [TestCase(Int32.MaxValue)]
		public void FSMLocalVar_DefineInt_ReturnsExpectedValue(Int32 value)
		{
			var sm = new FSM("FSM");
			sm.OldVars.DefineFloat(TestVar1, value);

			Assert.AreEqual(value, sm.OldVars[TestVar1].FloatValue);
		}

		/*
		[TestCase(false)] [TestCase(true)]
		public void FSMLocalVar_CreateBool_ReturnsExpectedValue(Boolean value)
		{
			var sm = new FSM("FSM");
			var boolVar = sm.Vars.CreateBool(TestVar1, value);

			Assert.AreEqual(value, boolVar.Value);
			Assert.AreEqual(value, sm.Vars.GetBool(TestVar1).Value);
			Assert.AreEqual(boolVar, sm.Vars.GetBool(TestVar1));
		}
	*/
	}
}
