// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.FSM.Tests
{
	public class StatemachineVariableTests
	{
		private static readonly String TestVar1 = "Test1";
		private static readonly String TestVar2 = "Test2";

		[Test]
		public void SMVars_UndefinedVariables_ReturnDefaultValues()
		{
			var sm = new Statemachine("Test FSM");
			sm.GlobalVars.Clear();
			Assert.AreEqual(false, sm.GlobalVars["undefined-bool"].BoolValue);
			Assert.AreEqual(0f, sm.GlobalVars["undefined-float"].FloatValue);
			Assert.AreEqual(0, sm.GlobalVars["undefined-int"].IntValue);

			Assert.AreEqual(false, sm.LocalVars["undefined-bool"].BoolValue);
			Assert.AreEqual(0f, sm.LocalVars["undefined-float"].FloatValue);
			Assert.AreEqual(0, sm.LocalVars["undefined-int"].IntValue);
		}

		[Test]
		public void SMVars_UndefinedVariables_CanBeAssignedTo()
		{
			Statemachine.Vars.Clear();
			Statemachine.Vars["undefined-bool"].BoolValue = true;
			Statemachine.Vars["undefined-float"].FloatValue = 1.2345f;
			Statemachine.Vars["undefined-int"].IntValue = -123456;
			Assert.AreEqual(true, Statemachine.Vars["undefined-bool"].BoolValue);
			Assert.AreEqual(1.2345f, Statemachine.Vars["undefined-float"].FloatValue);
			Assert.AreEqual(-123456, Statemachine.Vars["undefined-int"].IntValue);

			var sm = new Statemachine("Test FSM");
			sm.LocalVars["undefined-bool"].BoolValue = true;
			sm.LocalVars["undefined-float"].FloatValue = -1.2345f;
			sm.LocalVars["undefined-int"].IntValue = 123456;
			Assert.AreEqual(true, sm.LocalVars["undefined-bool"].BoolValue);
			Assert.AreEqual(-1.2345f, sm.LocalVars["undefined-float"].FloatValue);
			Assert.AreEqual(123456, sm.LocalVars["undefined-int"].IntValue);
		}

		[TestCase(false)] [TestCase(true)]
		public void SMLocalVar_DefineBool_ReturnsExpectedValue(Boolean value)
		{
			var sm = new Statemachine("Test FSM");

			sm.LocalVars.DefineBool(TestVar1, value);

			Assert.AreEqual(value, sm.LocalVars[TestVar1].BoolValue);
		}

		[TestCase(0f)] [TestCase(Single.MinValue)] [TestCase(Single.MaxValue)]
		public void SMLocalVar_DefineFloat_ReturnsExpectedValue(Single value)
		{
			var sm = new Statemachine("Test FSM");

			sm.LocalVars.DefineFloat(TestVar1, value);

			Assert.AreEqual(value, sm.LocalVars[TestVar1].FloatValue);
		}

		[TestCase(0)] [TestCase(Int32.MinValue)] [TestCase(Int32.MaxValue)]
		public void SMLocalVar_DefineInt_ReturnsExpectedValue(Int32 value)
		{
			var sm = new Statemachine("Test FSM");

			sm.LocalVars.DefineFloat(TestVar1, value);

			Assert.AreEqual(value, sm.LocalVars[TestVar1].FloatValue);
		}
	}
}
