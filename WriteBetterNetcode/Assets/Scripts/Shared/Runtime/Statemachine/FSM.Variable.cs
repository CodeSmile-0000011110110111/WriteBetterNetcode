// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		/// <summary>
		///     Encapsulated variable for use within and outside a statemachine.
		/// </summary>
		/// <remarks>
		///     Variables are part of a Statemachine and can be logged, inspected, debugged alongside with the FSM state.
		/// </remarks>
		public sealed class Variable : IEquatable<Variable>
		{
			private ValueType m_ValueType;
			private UnionValue32 m_Value;
			internal ValueType Type => m_ValueType;

			public Boolean BoolValue
			{
				get
				{
					SetTypeIfNone(ValueType.Bool);
					ThrowIfTypeMismatch(ValueType.Bool);
					return m_Value.BoolValue;
				}
				set
				{
					SetTypeIfNone(ValueType.Bool);
					ThrowIfTypeMismatch(ValueType.Bool);
					m_Value.BoolValue = value;
				}
			}
			public Single FloatValue
			{
				get
				{
					SetTypeIfNone(ValueType.Float);
					ThrowIfTypeMismatch(ValueType.Float);
					return m_Value.FloatValue;
				}
				set
				{
					SetTypeIfNone(ValueType.Float);
					ThrowIfTypeMismatch(ValueType.Float);
					m_Value.FloatValue = value;
				}
			}
			public Int32 IntValue
			{
				get
				{
					SetTypeIfNone(ValueType.Int);
					ThrowIfTypeMismatch(ValueType.Int);
					return m_Value.IntValue;
				}
				set
				{
					SetTypeIfNone(ValueType.Int);
					ThrowIfTypeMismatch(ValueType.Int);
					m_Value.IntValue = value;
				}
			}
			public static Boolean operator ==(Variable left, Variable right) => Equals(left, right);
			public static Boolean operator !=(Variable left, Variable right) => !Equals(left, right);
			public static Boolean operator >=(Variable left, Variable right) => left > right || Equals(left, right);
			public static Boolean operator <=(Variable left, Variable right) => left < right || Equals(left, right);
			public static Boolean operator >(Variable left, Variable right) => !(left < right);

			public static Boolean operator <(Variable left, Variable right)
			{
				if (left.m_ValueType != right.m_ValueType)
					throw new InvalidOperationException($"cannot compare different var types: {left} vs {right}");

				switch (left.m_ValueType)
				{
					case ValueType.None:
						throw new InvalidOperationException("cannot 'less than' untyped variables");
					case ValueType.Bool:
						throw new InvalidOperationException("cannot 'less than' boolean variables");
					case ValueType.Float:
						return left.FloatValue < right.FloatValue;
					case ValueType.Int:
						return left.IntValue < right.IntValue;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			public static Variable operator +(Variable left, Variable right) => left.m_ValueType switch
			{
				ValueType.Float => new Variable(left.m_ValueType, left.FloatValue + right.FloatValue),
				ValueType.Int => new Variable(left.m_ValueType, left.IntValue + right.IntValue),
				_ => throw new ArgumentOutOfRangeException(),
			};

			public static Variable operator -(Variable left, Variable right) => left.m_ValueType switch
			{
				ValueType.Float => new Variable(left.m_ValueType, left.FloatValue - right.FloatValue),
				ValueType.Int => new Variable(left.m_ValueType, left.IntValue - right.IntValue),
				_ => throw new ArgumentOutOfRangeException(),
			};

			public static Variable operator *(Variable left, Variable right) => left.m_ValueType switch
			{
				ValueType.Float => new Variable(left.m_ValueType, left.FloatValue * right.FloatValue),
				ValueType.Int => new Variable(left.m_ValueType, left.IntValue * right.IntValue),
				_ => throw new ArgumentOutOfRangeException(),
			};

			public static Variable operator /(Variable left, Variable right) => left.m_ValueType switch
			{
				ValueType.Float => new Variable(left.m_ValueType, left.FloatValue / right.FloatValue),
				ValueType.Int => new Variable(left.m_ValueType, left.IntValue / right.IntValue),
				_ => throw new ArgumentOutOfRangeException(),
			};

			public static Variable Bool(Boolean value) => new(ValueType.Bool, value);
			public static Variable Float(Single value) => new(ValueType.Float, value);
			public static Variable Int(Int32 value) => new(ValueType.Int, value);
			public static CompareCondition IsTrue(Variable variable) => new(variable, Bool(true));
			public static CompareCondition IsFalse(Variable variable) => new(variable, Bool(false));
			public static CompareCondition IsEqual(Variable variable, Int32 value) => new(variable, Int(value));

			public static ModifyAction SetTrue(Variable variable) => new(variable, Bool(true));
			public static ModifyAction SetFalse(Variable variable) => new(variable, Bool(false));
			public static ModifyAction SetInt(Variable variable, Int32 value) => new(variable, Int(value));

			internal Variable()
			{
				m_ValueType = ValueType.None;
				m_Value = new UnionValue32();
			}

			internal Variable(Variable other)
			{
				m_ValueType = other.m_ValueType;
				m_Value = other.m_Value;
			}

			private Variable(ValueType valueType, Boolean value)
			{
				m_ValueType = valueType;
				m_Value = new UnionValue32 { BoolValue = value };
			}

			private Variable(ValueType valueType, Single value)
			{
				m_ValueType = valueType;
				m_Value = new UnionValue32 { FloatValue = value };
			}

			private Variable(ValueType valueType, Int32 value)
			{
				m_ValueType = valueType;
				m_Value = new UnionValue32 { IntValue = value };
			}

			public Boolean Equals(Variable other)
			{
				if (ReferenceEquals(null, other))
					return false;
				if (ReferenceEquals(this, other))
					return true;

				if (m_ValueType != other.m_ValueType)
					throw new InvalidOperationException($"cannot compare different var types: {this} vs {other}");

				return m_Value.Equals(other.m_Value);
			}

			internal void Set(Variable operand)
			{
				ThrowIfTypeMismatch(operand.m_ValueType);
				m_Value = operand.m_Value;
			}

			internal void Add(Variable operand)
			{
				ThrowIfTypeMismatch(operand.m_ValueType);
				switch (m_ValueType)
				{
					case ValueType.Float:
						m_Value.FloatValue += operand.FloatValue;
						break;
					case ValueType.Int:
						m_Value.IntValue += operand.IntValue;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			internal void Sub(Variable operand)
			{
				ThrowIfTypeMismatch(operand.m_ValueType);
				switch (m_ValueType)
				{
					case ValueType.Float:
						m_Value.FloatValue -= operand.FloatValue;
						break;
					case ValueType.Int:
						m_Value.IntValue -= operand.IntValue;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			internal void Mul(Variable operand)
			{
				ThrowIfTypeMismatch(operand.m_ValueType);
				switch (m_ValueType)
				{
					case ValueType.Float:
						m_Value.FloatValue *= operand.FloatValue;
						break;
					case ValueType.Int:
						m_Value.IntValue *= operand.IntValue;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			internal void Div(Variable operand)
			{
				ThrowIfTypeMismatch(operand.m_ValueType);
				switch (m_ValueType)
				{
					case ValueType.Float:
						m_Value.FloatValue /= operand.FloatValue;
						break;
					case ValueType.Int:
						m_Value.IntValue /= operand.IntValue;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal void SetTypeIfNone(ValueType valueType)
			{
				if (m_ValueType == ValueType.None)
					m_ValueType = valueType;
			}

			private void ThrowIfTypeMismatch(ValueType expectedType)
			{
				// Note: In a release build, compiler optimization will strip calls to empty methods
#if DEBUG || DEVELOPMENT_BUILD
				if (m_ValueType != expectedType)
					throw new InvalidCastException($"Type mismatch: {m_ValueType} vs {expectedType}");
#endif
			}

			public override String ToString() => $"Variable({m_ValueType}:{m_Value})";
			public override Boolean Equals(Object obj) => ReferenceEquals(this, obj) || obj is Variable other && Equals(other);
			public override Int32 GetHashCode() => HashCode.Combine((Int32)m_ValueType, m_Value);

			[StructLayout(LayoutKind.Explicit)]
			internal struct UnionValue32
			{
				[FieldOffset(0)] public Boolean BoolValue;
				[FieldOffset(0)] public Single FloatValue;
				[FieldOffset(0)] public Int32 IntValue;
			}

			public sealed class CompareCondition : ICondition
			{
				public enum Comparator
				{
					Equal,
					GreaterThan,
					GreaterThanOrEqual,
					LessThan,
					LessThanOrEqual,
				}

				private readonly Variable m_Variable;
				private readonly Variable m_Comparand;
				private readonly Comparator m_Comparator;

				private CompareCondition() {} // forbidden default ctor

				internal CompareCondition(Variable variable, Variable comparand, Comparator comparator = Comparator.Equal)
				{
					if (comparand.Type == ValueType.Bool && comparator != Comparator.Equal)
						throw new ArgumentException($"Bool vars can only be compared for equality, not: {comparator}");

					m_Variable = variable;
					m_Comparand = comparand;
					m_Comparator = comparator;
				}

				public Boolean IsSatisfied(FSM sm)
				{
					switch (m_Comparator)
					{
						case Comparator.Equal:
							if (m_Variable.Type == ValueType.Float)
								return Mathf.Approximately(m_Variable.FloatValue, m_Comparand.FloatValue);

							return m_Variable == m_Comparand;
						case Comparator.GreaterThan:
							return m_Variable > m_Comparand;
						case Comparator.GreaterThanOrEqual:
							return m_Variable >= m_Comparand;
						case Comparator.LessThan:
							return m_Variable < m_Comparand;
						case Comparator.LessThanOrEqual:
							return m_Variable <= m_Comparand;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			public sealed class ModifyAction : IAction
			{
				public enum Operator
				{
					Set,
					Add,
					Subtract,
					Multiply,
					Divide,
					// Negate,
				}

				private readonly Variable m_Variable;
				private readonly Variable m_Operand;
				private readonly Operator m_Operator;

				private ModifyAction() {} // forbidden default ctor

				internal ModifyAction(Variable variable, Variable operand, Operator @operator = Operator.Set)
				{
					if (operand.Type == ValueType.Bool && @operator != Operator.Set)
						throw new ArgumentException($"Invalid operator for Bool vars: {@operator}");
					// if (@operator == Operator.Negate && operand.Type != ValueType.Bool)
					// 	throw new ArgumentException($"Invalid operator for non-Bool vars: {@operator}");

					m_Variable = variable;
					m_Operand = operand;
					m_Operator = @operator;
				}

				public void Execute(FSM sm)
				{
					switch (m_Operator)
					{
						case Operator.Set:
							m_Variable.Set(m_Operand);
							break;
						case Operator.Add:
							m_Variable.Add(m_Operand);
							break;
						case Operator.Subtract:
							m_Variable.Sub(m_Operand);
							break;
						case Operator.Multiply:
							m_Variable.Mul(m_Operand);
							break;
						case Operator.Divide:
							m_Variable.Div(m_Operand);
							break;
						// case Operator.Negate:
						// 	m_Variable.BoolValue = !m_Variable.BoolValue;
						// 	break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			internal enum ValueType
			{
				None,
				Bool,
				Float,
				Int,
			}
		}
	}
}
