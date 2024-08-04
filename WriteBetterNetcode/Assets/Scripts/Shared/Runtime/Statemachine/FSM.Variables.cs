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
		public enum VariableScope
		{
			Local,
			Global,
		}

		public abstract class VariableBase {}

		public sealed class BoolVariable : VariableBase, IEquatable<BoolVariable>
		{
			public Boolean Value { get; set; }
			public static Boolean operator ==(BoolVariable left, BoolVariable right) => Equals(left, right);
			public static Boolean operator !=(BoolVariable left, BoolVariable right) => !Equals(left, right);
			public BoolVariable(Boolean value = default) => Value = value;
			public Boolean Equals(BoolVariable other) => other != null && Value == other.Value;
			public override Boolean Equals(Object obj) => obj is BoolVariable other && Equals(other);
			public override Int32 GetHashCode() => Value.GetHashCode();
		}

		public sealed class IntVariable : VariableBase, IEquatable<IntVariable>
		{
			public Int32 Value { get; set; }
			public static Boolean operator ==(IntVariable left, IntVariable right) => Equals(left, right);
			public static Boolean operator !=(IntVariable left, IntVariable right) => !Equals(left, right);
			public IntVariable(Int32 value = default) => Value = value;
			public Boolean Equals(IntVariable other) => other != null && Value == other.Value;
			public override Boolean Equals(Object obj) => obj is IntVariable other && Equals(other);
			public override Int32 GetHashCode() => Value;
		}

		public sealed class FloatVariable : VariableBase, IEquatable<FloatVariable>
		{
			public Single Value { get; set; }
			public static Boolean operator ==(FloatVariable left, FloatVariable right) => Equals(left, right);
			public static Boolean operator !=(FloatVariable left, FloatVariable right) => !Equals(left, right);
			public FloatVariable(Single value = default) => Value = value;
			public Boolean Equals(FloatVariable other) => other != null && Value == other.Value;
			public override Boolean Equals(Object obj) => obj is FloatVariable other && Equals(other);
			public override Int32 GetHashCode() => Value.GetHashCode();
		}

		public sealed class StructVariable<T> : VariableBase, IEquatable<StructVariable<T>> where T : struct
		{
			public T Value { get; set; }
			public static Boolean operator ==(StructVariable<T> left, StructVariable<T> right) => Equals(left, right);
			public static Boolean operator !=(StructVariable<T> left, StructVariable<T> right) => !Equals(left, right);
			public StructVariable(T value = default) => Value = value;
			public Boolean Equals(StructVariable<T> other) => other != null && Value.Equals(other.Value);
			public override Boolean Equals(Object obj) => obj is StructVariable<T> other && Equals(other);
			public override Int32 GetHashCode() => Value.GetHashCode();
		}

		public class Variables
		{
			private readonly Dictionary<String, VariableBase> m_Variables = new();

			public VariableBase this[String variableName] => m_Variables[variableName];

			public void Clear() => m_Variables.Clear();


			internal String FindVariableName(VariableBase variable) =>
				m_Variables.FirstOrDefault(kvp => kvp.Value == variable).Key;

			public BoolVariable DefineBool(String name, Boolean value = default)
			{
				var boolVar = new BoolVariable(value);
				m_Variables.Add(name, boolVar);
				return boolVar;
			}

			public BoolVariable GetBool(String name) => m_Variables[name] as BoolVariable;

			public StructVariable<T> DefineStruct<T>(String name, T value) where T : struct
			{
				var structVar = new StructVariable<T>(value);
				m_Variables.Add(name, structVar);
				return structVar;
			}

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
