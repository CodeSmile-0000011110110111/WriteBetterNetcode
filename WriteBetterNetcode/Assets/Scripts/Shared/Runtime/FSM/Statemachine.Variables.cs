// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.FSM
{
	public sealed partial class Statemachine
	{
		public enum VariableType
		{
			Bool,
			Float,
			Int,
			UInt,
		}

		public class Variables
		{
			private readonly Dictionary<String, Variable> m_Variables = new();

			public Variable this[String variableName] =>
				m_Variables.TryGetValue(variableName, out var variable) ? variable : null;

			public Boolean Add(String name, Variable variable) => m_Variables.TryAdd(name, variable);
		}

		public sealed class Variable
		{
			private readonly VariableType m_VariableType;
			private UnionVariable32 m_Variable;

			public Boolean BoolValue
			{
				get
				{
					ThrowIfTypeMismatch(VariableType.Bool);
					return m_Variable.BoolValue;
				}
				set
				{
					ThrowIfTypeMismatch(VariableType.Bool);
					m_Variable.BoolValue = value;
				}
			}

			public Single FloatValue
			{
				get
				{
					ThrowIfTypeMismatch(VariableType.Float);
					return m_Variable.FloatValue;
				}
				set
				{
					ThrowIfTypeMismatch(VariableType.Float);
					m_Variable.FloatValue = value;
				}
			}

			public Int32 IntValue
			{
				get
				{
					ThrowIfTypeMismatch(VariableType.Int);
					return m_Variable.Int32Value;
				}
				set
				{
					ThrowIfTypeMismatch(VariableType.Int);
					m_Variable.Int32Value = value;
				}
			}

			public UInt32 UIntValue
			{
				get
				{
					ThrowIfTypeMismatch(VariableType.UInt);
					return m_Variable.UInt32Value;
				}
				set
				{
					ThrowIfTypeMismatch(VariableType.UInt);
					m_Variable.UInt32Value = value;
				}
			}

			public static Variable Bool(Boolean value) => new(VariableType.Bool, new UnionVariable32 { BoolValue = value });
			public static Variable Float(Single value) => new(VariableType.Float, new UnionVariable32 { FloatValue = value });
			public static Variable Int(Int32 value) => new(VariableType.Int, new UnionVariable32 { Int32Value = value });
			public static Variable UInt(UInt32 value) => new(VariableType.UInt, new UnionVariable32 { UInt32Value = value });

			private Variable(VariableType variableType, UnionVariable32 variable = default)
			{
				m_VariableType = variableType;
				m_Variable = variable;
			}

			private void ThrowIfTypeMismatch(VariableType expectedType)
			{
				// Note: In a release build, calls to an empty method will get stripped
#if DEBUG || DEVELOPMENT_BUILD
				if (m_VariableType != expectedType)
				{
					throw new InvalidCastException(
						$"Variable is of type {m_VariableType}, cannot cast to/from: {expectedType}");
				}
#endif
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct UnionVariable32
		{
			[FieldOffset(0)] public Boolean BoolValue;
			[FieldOffset(0)] public Single FloatValue;
			[FieldOffset(0)] public Int32 Int32Value;
			[FieldOffset(0)] public UInt32 UInt32Value;
		}
	}
}
