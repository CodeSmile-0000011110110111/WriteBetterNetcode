// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.Core.Statemachine
{
	public class Variables
	{
		private readonly Dictionary<String, VariableBase> m_Variables = new();

		public VariableBase this[String variableName] => m_Variables[variableName];

		public void Clear() => m_Variables.Clear();

		internal String FindVariableName(VariableBase variable) => m_Variables.FirstOrDefault(kvp => kvp.Value == variable).Key;

		private T AddVariable<T>(String name, T variable) where T : VariableBase
		{
			m_Variables.Add(name, variable);
			return variable;
		}

		public BoolVar DefineBool(String name, Boolean value = default) => AddVariable(name, new BoolVar(value));

		public BoolVar GetBool(String name) => m_Variables[name] as BoolVar;

		public IntVar DefineInt(String name, Int32 value = default) => AddVariable(name, new IntVar(value));

		public IntVar GetInt(String name) => m_Variables[name] as IntVar;

		public FloatVar DefineFloat(String name, Single value = default) => AddVariable(name, new FloatVar(value));

		public FloatVar DefineFloat(String name, Int32 value) => AddVariable(name, new FloatVar(value));

		public FloatVar GetFloat(String name) => m_Variables[name] as FloatVar;

		public Var<T> DefineVar<T>() where T : struct => AddVariable(typeof(T).Name, new Var<T>());
		public Var<T> DefineVar<T>(String name, T value = default) where T : struct => AddVariable(name, new Var<T>(value));

		public Var<T> GetVar<T>(String name) where T : struct => m_Variables[name] as Var<T>;
	}

	public abstract class VariableBase
	{
		public static Boolean operator ==(VariableBase left, VariableBase right) => Equals(left, right);
		public static Boolean operator !=(VariableBase left, VariableBase right) => !Equals(left, right);

		public static Boolean operator <(VariableBase left, VariableBase right)
		{
			if (left is IntVar leftInt && right is IntVar rightInt)
				return leftInt < rightInt;
			if (left is FloatVar leftFloat && right is FloatVar rightFloat)
				return leftFloat < rightFloat;

			throw new InvalidOperationException(GetCompareExceptionMessage(left, right, "<"));
		}

		public static Boolean operator >(VariableBase left, VariableBase right)
		{
			if (left is IntVar leftInt && right is IntVar rightInt)
				return leftInt > rightInt;
			if (left is FloatVar leftFloat && right is FloatVar rightFloat)
				return leftFloat > rightFloat;

			throw new InvalidOperationException(GetCompareExceptionMessage(left, right, ">"));
		}

		public static Boolean operator <=(VariableBase left, VariableBase right)
		{
			if (left is IntVar leftInt && right is IntVar rightInt)
				return leftInt <= rightInt;
			if (left is FloatVar leftFloat && right is FloatVar rightFloat)
				return leftFloat <= rightFloat;

			throw new InvalidOperationException(GetCompareExceptionMessage(left, right, "<="));
		}

		public static Boolean operator >=(VariableBase left, VariableBase right)
		{
			if (left is IntVar leftInt && right is IntVar rightInt)
				return leftInt >= rightInt;
			if (left is FloatVar leftFloat && right is FloatVar rightFloat)
				return leftFloat >= rightFloat;

			throw new InvalidOperationException(GetCompareExceptionMessage(left, right, ">="));
		}

		private static String GetCompareExceptionMessage(VariableBase left, VariableBase right, String op) =>
			$"cannot compare: {left?.GetType().Name}({left}) {op} {right?.GetType().Name}({right})";

		public override Boolean Equals(Object obj) => throw new NotImplementedException();
		public override Int32 GetHashCode() => throw new NotImplementedException();

		public abstract void SetValue(VariableBase variable);
		public abstract void AddValue(VariableBase variable);
		public abstract void SubtractValue(VariableBase variable);
		public abstract void MultiplyValue(VariableBase variable);
		public abstract void DivideValue(VariableBase variable);
	}

	public sealed class BoolVar : VariableBase, IEquatable<BoolVar>
	{
		public Boolean Value { get; set; }
		public BoolVar(Boolean value = default) => Value = value;
		public Boolean Equals(BoolVar other) => !ReferenceEquals(null, other) && Value == other.Value;
		public override Boolean Equals(Object obj) => obj is BoolVar other && Equals(other);
		public override Int32 GetHashCode() => Value.GetHashCode();
		public override String ToString() => Value.ToString();

		public override void SetValue(VariableBase variable) => Value = ((BoolVar)variable).Value;
		public override void AddValue(VariableBase variable) => throw new NotSupportedException();
		public override void SubtractValue(VariableBase variable) => throw new NotSupportedException();
		public override void MultiplyValue(VariableBase variable) => throw new NotSupportedException();
		public override void DivideValue(VariableBase variable) => throw new NotSupportedException();
	}

	public sealed class IntVar : VariableBase, IEquatable<IntVar>
	{
		public Int32 Value { get; set; }
		public static Boolean operator <(IntVar left, IntVar right) => left.Value < right.Value;
		public static Boolean operator >(IntVar left, IntVar right) => left.Value > right.Value;
		public static Boolean operator <=(IntVar left, IntVar right) => left.Value <= right.Value;
		public static Boolean operator >=(IntVar left, IntVar right) => left.Value >= right.Value;
		public IntVar(Int32 value = default) => Value = value;
		public Boolean Equals(IntVar other) => !ReferenceEquals(null, other) && Value.Equals(other.Value);
		public override Boolean Equals(Object obj) => obj is IntVar other && Equals(other);
		public override Int32 GetHashCode() => Value;
		public override String ToString() => Value.ToString();

		public override void SetValue(VariableBase variable) => Value = ((IntVar)variable).Value;
		public override void AddValue(VariableBase variable) => Value += ((IntVar)variable).Value;
		public override void SubtractValue(VariableBase variable) => Value -= ((IntVar)variable).Value;
		public override void MultiplyValue(VariableBase variable) => Value *= ((IntVar)variable).Value;
		public override void DivideValue(VariableBase variable) => Value /= ((IntVar)variable).Value;
	}

	public sealed class FloatVar : VariableBase, IEquatable<FloatVar>
	{
		public Single Value { get; set; }
		public static Boolean operator <(FloatVar left, FloatVar right) => left.Value < right.Value;
		public static Boolean operator >(FloatVar left, FloatVar right) => left.Value > right.Value;
		public static Boolean operator <=(FloatVar left, FloatVar right) => left.Value <= right.Value;
		public static Boolean operator >=(FloatVar left, FloatVar right) => left.Value >= right.Value;
		public FloatVar(Single value = default) => Value = value;
		public Boolean Equals(FloatVar other) => !ReferenceEquals(null, other) && Value.Equals(other.Value);
		public override Boolean Equals(Object obj) => obj is FloatVar other && Equals(other);
		public override Int32 GetHashCode() => Value.GetHashCode();
		public override String ToString() => Value.ToString();

		public override void SetValue(VariableBase variable) => Value = ((FloatVar)variable).Value;
		public override void AddValue(VariableBase variable) => Value += ((FloatVar)variable).Value;
		public override void SubtractValue(VariableBase variable) => Value -= ((FloatVar)variable).Value;
		public override void MultiplyValue(VariableBase variable) => Value *= ((FloatVar)variable).Value;
		public override void DivideValue(VariableBase variable) => Value /= ((FloatVar)variable).Value;
	}

	public sealed class Var<T> : VariableBase, IEquatable<Var<T>> where T : struct
	{
		public T Value { get; set; }
		public Var(T value = default) => Value = value;
		public Boolean Equals(Var<T> other) => !ReferenceEquals(null, other) && Value.Equals(other.Value);
		public override Boolean Equals(Object obj) => obj is Var<T> other && Equals(other);
		public override Int32 GetHashCode() => Value.GetHashCode();
		public override String ToString() => Value.ToString();

		public override void SetValue(VariableBase variable) => Value = ((Var<T>)variable).Value;
		public override void AddValue(VariableBase variable) => throw new NotSupportedException();
		public override void SubtractValue(VariableBase variable) => throw new NotSupportedException();
		public override void MultiplyValue(VariableBase variable) => throw new NotSupportedException();
		public override void DivideValue(VariableBase variable) => throw new NotSupportedException();
	}
}
