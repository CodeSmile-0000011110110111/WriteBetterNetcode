// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		public abstract class VariableBase
		{
			public static Boolean operator ==(VariableBase left, VariableBase right) => Equals(left, right);
			public static Boolean operator !=(VariableBase left, VariableBase right) => !Equals(left, right);

			public static Boolean operator <(VariableBase left, VariableBase right)
			{
				if (left is IntVariable leftInt && right is IntVariable rightInt)
					return leftInt < rightInt;
				if (left is FloatVariable leftFloat && right is FloatVariable rightFloat)
					return leftFloat < rightFloat;

				throw new InvalidOperationException(GetCompareExceptionMessage(left, right, "<"));
			}

			public static Boolean operator >(VariableBase left, VariableBase right)
			{
				if (left is IntVariable leftInt && right is IntVariable rightInt)
					return leftInt > rightInt;
				if (left is FloatVariable leftFloat && right is FloatVariable rightFloat)
					return leftFloat > rightFloat;

				throw new InvalidOperationException(GetCompareExceptionMessage(left, right, ">"));
			}

			public static Boolean operator <=(VariableBase left, VariableBase right)
			{
				if (left is IntVariable leftInt && right is IntVariable rightInt)
					return leftInt <= rightInt;
				if (left is FloatVariable leftFloat && right is FloatVariable rightFloat)
					return leftFloat <= rightFloat;

				throw new InvalidOperationException(GetCompareExceptionMessage(left, right, "<="));
			}

			public static Boolean operator >=(VariableBase left, VariableBase right)
			{
				if (left is IntVariable leftInt && right is IntVariable rightInt)
					return leftInt >= rightInt;
				if (left is FloatVariable leftFloat && right is FloatVariable rightFloat)
					return leftFloat >= rightFloat;

				throw new InvalidOperationException(GetCompareExceptionMessage(left, right, ">="));
			}

			private static String GetCompareExceptionMessage(VariableBase left, VariableBase right, String op) =>
				$"cannot compare: {left?.GetType().Name}({left}) {op} {right?.GetType().Name}({right})";

			public abstract void SetValue(VariableBase variable);
			public abstract void AddValue(VariableBase variable);
			public abstract void SubtractValue(VariableBase variable);
			public abstract void MultiplyValue(VariableBase variable);
			public abstract void DivideValue(VariableBase variable);
		}

		public sealed class BoolVariable : VariableBase, IEquatable<BoolVariable>
		{
			public Boolean Value { get; set; }
			public BoolVariable(Boolean value = default) => Value = value;
			public Boolean Equals(BoolVariable other) => !ReferenceEquals(null, other) && Value == other.Value;
			public override Boolean Equals(Object obj) => obj is BoolVariable other && Equals(other);
			public override Int32 GetHashCode() => Value.GetHashCode();
			public override String ToString() => Value.ToString();

			public override void SetValue(VariableBase variable) => Value = ((BoolVariable)variable).Value;
			public override void AddValue(VariableBase variable) => throw new NotSupportedException();
			public override void SubtractValue(VariableBase variable) => throw new NotSupportedException();
			public override void MultiplyValue(VariableBase variable) => throw new NotSupportedException();
			public override void DivideValue(VariableBase variable) => throw new NotSupportedException();
		}

		public sealed class IntVariable : VariableBase, IEquatable<IntVariable>
		{
			public Int32 Value { get; set; }
			public static Boolean operator <(IntVariable left, IntVariable right) => left.Value < right.Value;
			public static Boolean operator >(IntVariable left, IntVariable right) => left.Value > right.Value;
			public static Boolean operator <=(IntVariable left, IntVariable right) => left.Value <= right.Value;
			public static Boolean operator >=(IntVariable left, IntVariable right) => left.Value >= right.Value;
			public IntVariable(Int32 value = default) => Value = value;
			public Boolean Equals(IntVariable other) => !ReferenceEquals(null, other) && Value.Equals(other.Value);
			public override Boolean Equals(Object obj) => obj is IntVariable other && Equals(other);
			public override Int32 GetHashCode() => Value;
			public override String ToString() => Value.ToString();

			public override void SetValue(VariableBase variable) => Value = ((IntVariable)variable).Value;
			public override void AddValue(VariableBase variable) => Value += ((IntVariable)variable).Value;
			public override void SubtractValue(VariableBase variable) => Value -= ((IntVariable)variable).Value;
			public override void MultiplyValue(VariableBase variable) => Value *= ((IntVariable)variable).Value;
			public override void DivideValue(VariableBase variable) => Value /= ((IntVariable)variable).Value;
		}

		public sealed class FloatVariable : VariableBase, IEquatable<FloatVariable>
		{
			public Single Value { get; set; }
			public static Boolean operator <(FloatVariable left, FloatVariable right) => left.Value < right.Value;
			public static Boolean operator >(FloatVariable left, FloatVariable right) => left.Value > right.Value;
			public static Boolean operator <=(FloatVariable left, FloatVariable right) => left.Value <= right.Value;
			public static Boolean operator >=(FloatVariable left, FloatVariable right) => left.Value >= right.Value;
			public FloatVariable(Single value = default) => Value = value;
			public Boolean Equals(FloatVariable other) => !ReferenceEquals(null, other) && Value.Equals(other.Value);
			public override Boolean Equals(Object obj) => obj is FloatVariable other && Equals(other);
			public override Int32 GetHashCode() => Value.GetHashCode();
			public override String ToString() => Value.ToString();

			public override void SetValue(VariableBase variable) => Value = ((FloatVariable)variable).Value;
			public override void AddValue(VariableBase variable) => Value += ((FloatVariable)variable).Value;
			public override void SubtractValue(VariableBase variable) => Value -= ((FloatVariable)variable).Value;
			public override void MultiplyValue(VariableBase variable) => Value *= ((FloatVariable)variable).Value;
			public override void DivideValue(VariableBase variable) => Value /= ((FloatVariable)variable).Value;
		}

		public sealed class StructVariable<T> : VariableBase, IEquatable<StructVariable<T>> where T : struct
		{
			public T Value { get; set; }
			public StructVariable(T value = default) => Value = value;
			public Boolean Equals(StructVariable<T> other) => !ReferenceEquals(null, other) && Value.Equals(other.Value);
			public override Boolean Equals(Object obj) => obj is StructVariable<T> other && Equals(other);
			public override Int32 GetHashCode() => Value.GetHashCode();
			public override String ToString() => Value.ToString();

			public override void SetValue(VariableBase variable) => Value = ((StructVariable<T>)variable).Value;
			public override void AddValue(VariableBase variable) => throw new NotSupportedException();
			public override void SubtractValue(VariableBase variable) => throw new NotSupportedException();
			public override void MultiplyValue(VariableBase variable) => throw new NotSupportedException();
			public override void DivideValue(VariableBase variable) => throw new NotSupportedException();
		}

		public class Variables
		{
			private readonly Dictionary<String, VariableBase> m_Variables = new();

			public VariableBase this[String variableName] => m_Variables[variableName];

			public void Clear() => m_Variables.Clear();

			internal String FindVariableName(VariableBase variable) =>
				m_Variables.FirstOrDefault(kvp => kvp.Value == variable).Key;

			private T AddVariable<T>(String name, T variable) where T : VariableBase
			{
				m_Variables.Add(name, variable);
				return variable;
			}

			public BoolVariable DefineBool(String name, Boolean value = default) => AddVariable(name, new BoolVariable(value));

			public BoolVariable GetBool(String name) => m_Variables[name] as BoolVariable;

			public IntVariable DefineInt(String name, Int32 value = default) => AddVariable(name, new IntVariable(value));

			public IntVariable GetInt(String name) => m_Variables[name] as IntVariable;

			public FloatVariable DefineFloat(String name, Single value = default) =>
				AddVariable(name, new FloatVariable(value));

			public FloatVariable DefineFloat(String name, Int32 value) => AddVariable(name, new FloatVariable(value));

			public FloatVariable GetFloat(String name) => m_Variables[name] as FloatVariable;

			public StructVariable<T> DefineStruct<T>(String name, T value = default) where T : struct =>
				AddVariable(name, new StructVariable<T>(value));

			public StructVariable<T> GetStruct<T>(String name) where T : struct => m_Variables[name] as StructVariable<T>;
		}

		public class OldVariables
		{
			private readonly Dictionary<String, OldVar> m_Variables = new();

			public OldVar this[String variableName]
			{
				get
				{
					if (m_Variables.TryGetValue(variableName, out var variable))
						return variable;

					// create a new untyped variable instead
					variable = new OldVar();
					m_Variables.Add(variableName, variable);
					return variable;
				}
			}

			internal OldVariables() {} // forbidden default ctor

			public OldVar DefineBool(String name, Boolean value = false)
			{
				ThrowIfVariableNameAlreadyExists(name);

				var variable = OldVar.Bool(value);
				m_Variables.Add(name, variable);
				return variable;
			}

			public OldVar DefineFloat(String name, Single value = 0f)
			{
				ThrowIfVariableNameAlreadyExists(name);

				var variable = OldVar.Float(value);
				m_Variables.Add(name, variable);
				return variable;
			}

			public OldVar DefineInt(String name, Int32 value = 0)
			{
				ThrowIfVariableNameAlreadyExists(name);

				var variable = OldVar.Int(value);
				m_Variables.Add(name, variable);
				return variable;
			}

			public void Clear() => m_Variables.Clear();

			internal String FindName(OldVar variable) => m_Variables.FirstOrDefault(kvp => kvp.Value == variable).Key;

			private void ThrowIfVariableNameAlreadyExists(String name)
			{
				if (m_Variables.ContainsKey(name))
					throw new ArgumentException($"Variable named '{name}' already exists");
			}
		}
	}
}
