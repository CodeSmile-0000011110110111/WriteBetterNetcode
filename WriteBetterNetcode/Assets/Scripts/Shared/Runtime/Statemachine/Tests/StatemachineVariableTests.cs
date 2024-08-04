// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Actions;
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
		private const String TestVar2 = "TestVar2";

		[SetUp]
		public void SetUp() => new FSM("FSM").StaticVars.Clear();

		[TestCase(false)] [TestCase(true)]
		public void FSM_DefineBool_ReturnsExpectedValue(Boolean value)
		{
			var sm = new FSM("FSM");
			sm.Vars.DefineBool(TestVar1, value);
			sm.StaticVars.DefineBool(TestVar1, value);

			Assert.AreEqual(value, sm.Vars.GetBool(TestVar1).Value);
			Assert.AreEqual(value, sm.StaticVars.GetBool(TestVar1).Value);
		}

		[TestCase(0)] [TestCase(Int32.MinValue)] [TestCase(Int32.MaxValue)]
		public void FSM_DefineInt_ReturnsExpectedValue(Int32 value)
		{
			var sm = new FSM("FSM");
			sm.Vars.DefineInt(TestVar1, value);
			sm.StaticVars.DefineInt(TestVar1, value);

			Assert.AreEqual(value, sm.Vars.GetInt(TestVar1).Value);
			Assert.AreEqual(value, sm.StaticVars.GetInt(TestVar1).Value);
		}

		[TestCase(0f)] [TestCase(Single.MinValue)] [TestCase(Single.MaxValue)]
		public void FSM_DefineFloat_ReturnsExpectedValue(Single value)
		{
			var sm = new FSM("FSM");
			sm.Vars.DefineFloat(TestVar1, value);
			sm.StaticVars.DefineFloat(TestVar1, value);

			Assert.AreEqual(value, sm.Vars.GetFloat(TestVar1).Value);
			Assert.AreEqual(value, sm.StaticVars.GetFloat(TestVar1).Value);
		}

		[Test]
		public void FSM_DefineStruct_ReturnsExpectedValue()
		{
			var value = new Vector3(0f, Single.MinValue, Single.MaxValue);
			var sm = new FSM("FSM");
			sm.Vars.DefineStruct(TestVar1, value);
			sm.StaticVars.DefineStruct(TestVar1, value);

			Assert.AreEqual(value, sm.Vars.GetStruct<Vector3>(TestVar1).Value);
			Assert.AreEqual(value, sm.StaticVars.GetStruct<Vector3>(TestVar1).Value);
		}

		// Bool vars
		[TestCase(true, true)] [TestCase(false, false)]
		public void BoolVar_IsVarTrue_AsExpected(Boolean value, Boolean expected) =>
			Assert.AreEqual(expected, new IsTrue(new FSM.BoolVariable(value)).IsSatisfied(null));

		[TestCase(true, false)] [TestCase(false, true)]
		public void BoolVar_IsVarFalse_AsExpected(Boolean value, Boolean expected) =>
			Assert.AreEqual(expected, new IsFalse(new FSM.BoolVariable(value)).IsSatisfied(null));

		[TestCase(false, false, true)] [TestCase(true, false, false)]
		[TestCase(false, true, false)] [TestCase(true, true, true)]
		public void BoolVar_IsVarEqual_AsExpected(Boolean v1, Boolean v2, Boolean expected) => Assert.AreEqual(expected,
			new IsEqual(new FSM.BoolVariable(v1), new FSM.BoolVariable(v2)).IsSatisfied(null));

		[TestCase(false, false, false)] [TestCase(true, false, true)]
		[TestCase(false, true, true)] [TestCase(true, true, false)]
		public void BoolVar_IsVarNotEqual_AsExpected(Boolean v1, Boolean v2, Boolean expected) =>
			Assert.AreEqual(expected, new IsNotEqual(new FSM.BoolVariable(v1), v2).IsSatisfied(null));

		[Test] public void BoolVar_SetVarTrue_IsTrue()
		{
			var boolVar = new FSM.BoolVariable();
			new SetTrue(boolVar).Execute(null);
			Assert.IsTrue(boolVar.Value);
		}

		[Test] public void BoolVar_SetVarFalse_IsFalse()
		{
			var boolVar = new FSM.BoolVariable(true);
			new SetFalse(boolVar).Execute(null);
			Assert.IsFalse(boolVar.Value);
		}

		// Int vars
		[TestCase(0, 0, true)] [TestCase(-1, 0, false)]
		[TestCase(0, 1, false)] [TestCase(Int32.MinValue, Int32.MinValue, true)]
		public void IntVar_IsVarEqual_AsExpected(Int32 v1, Int32 v2, Boolean expected) =>
			Assert.AreEqual(expected, new IsEqual(new FSM.IntVariable(v1), v2).IsSatisfied(null));

		[TestCase(0, 0, false)] [TestCase(-1, 0, true)]
		[TestCase(0, 1, true)] [TestCase(Int32.MinValue, Int32.MinValue, false)]
		public void IntVar_IsVarNotEqual_AsExpected(Int32 v1, Int32 v2, Boolean expected) =>
			Assert.AreEqual(expected, new IsNotEqual(new FSM.IntVariable(v1), v2).IsSatisfied(null));

		[TestCase(0, 0, false)] [TestCase(0, -1, true)]
		[TestCase(0, 1, false)] [TestCase(Int32.MinValue, Int32.MinValue, false)]
		public void IntVar_IsGreater_AsExpected(Int32 v1, Int32 v2, Boolean expected) =>
			Assert.AreEqual(expected, new IsGreater(new FSM.IntVariable(v1), v2).IsSatisfied(null));

		[TestCase(0, 0, true)] [TestCase(0, -1, true)]
		[TestCase(0, 1, false)] [TestCase(Int32.MinValue, Int32.MinValue, true)]
		public void IntVar_IsGreaterOrEqual_AsExpected(Int32 v1, Int32 v2, Boolean expected) => Assert.AreEqual(expected,
			new IsGreaterOrEqual(new FSM.IntVariable(v1), v2).IsSatisfied(null));

		[TestCase(0, 0, false)] [TestCase(0, -1, false)]
		[TestCase(0, 1, true)] [TestCase(Int32.MinValue, Int32.MinValue, false)]
		public void IntVar_IsLess_AsExpected(Int32 v1, Int32 v2, Boolean expected) =>
			Assert.AreEqual(expected, new IsLess(new FSM.IntVariable(v1), v2).IsSatisfied(null));

		[TestCase(0, 0, true)] [TestCase(0, -1, false)]
		[TestCase(0, 1, true)] [TestCase(Int32.MinValue, Int32.MinValue, true)]
		public void IntVar_IsLessOrEqual_AsExpected(Int32 v1, Int32 v2, Boolean expected) => Assert.AreEqual(expected,
			new IsLessOrEqual(new FSM.IntVariable(v1), v2).IsSatisfied(null));

		[Test] public void IntVar_SetVarValue_AsExpected()
		{
			var intVar = new FSM.IntVariable();

			new Assign(intVar, 1).Execute(null);
			Assert.AreEqual(1, intVar.Value);

			new Assign(intVar, new FSM.IntVariable(2)).Execute(null);
			Assert.AreEqual(2, intVar.Value);
		}

		// Float vars
		[TestCase(0f, 0f, true)] [TestCase(-1.2345f, 0f, false)]
		[TestCase(0f, 1.2345f, false)] [TestCase(Single.MinValue, Single.MinValue, true)]
		public void FloatVar_IsVarEqual_AsExpected(Single v1, Single v2, Boolean expected) =>
			Assert.AreEqual(expected, new IsEqual(new FSM.FloatVariable(v1), v2).IsSatisfied(null));

		[TestCase(0f, 0f, false)] [TestCase(-1.2345f, 0f, true)]
		[TestCase(0f, 1.2345f, true)] [TestCase(Single.MinValue, Single.MinValue, false)]
		public void FloatVar_IsVarNotEqual_AsExpected(Single v1, Single v2, Boolean expected) =>
			Assert.AreEqual(expected, new IsNotEqual(new FSM.FloatVariable(v1), v2).IsSatisfied(null));

		[TestCase(0f, 0f, false)] [TestCase(0f, -1f, true)]
		[TestCase(0f, 1f, false)] [TestCase(Single.MinValue, Single.MinValue, false)]
		public void FloatVar_IsGreater_AsExpected(Single v1, Single v2, Boolean expected) =>
			Assert.AreEqual(expected, new IsGreater(new FSM.FloatVariable(v1), v2).IsSatisfied(null));

		[TestCase(0f, 0f, true)] [TestCase(0f, -1f, true)]
		[TestCase(0f, 1f, false)] [TestCase(Single.MinValue, Single.MinValue, true)]
		public void FloatVar_IsGreaterOrEqual_AsExpected(Single v1, Single v2, Boolean expected) => Assert.AreEqual(expected,
			new IsGreaterOrEqual(new FSM.FloatVariable(v1), v2).IsSatisfied(null));

		[TestCase(0f, 0f, false)] [TestCase(0f, -1f, false)]
		[TestCase(0f, 1f, true)] [TestCase(Single.MinValue, Single.MinValue, false)]
		public void FloatVar_IsLess_AsExpected(Single v1, Single v2, Boolean expected) =>
			Assert.AreEqual(expected, new IsLess(new FSM.FloatVariable(v1), v2).IsSatisfied(null));

		[TestCase(0f, 0f, true)] [TestCase(0f, -1f, false)]
		[TestCase(0f, 1f, true)] [TestCase(Single.MinValue, Single.MinValue, true)]
		public void FloatVar_IsLessOrEqual_AsExpected(Single v1, Single v2, Boolean expected) => Assert.AreEqual(expected,
			new IsLessOrEqual(new FSM.FloatVariable(v1), v2).IsSatisfied(null));

		[Test] public void FloatVar_SetVarValue_AsExpected()
		{
			var floatVar = new FSM.FloatVariable();
			var expected = -1.2345f;

			new Assign(floatVar, expected).Execute(null);
			Assert.AreEqual(expected, floatVar.Value);

			new Assign(floatVar, new FSM.FloatVariable(expected * 2f)).Execute(null);
			Assert.AreEqual(expected * 2f, floatVar.Value);
		}
	}
}
