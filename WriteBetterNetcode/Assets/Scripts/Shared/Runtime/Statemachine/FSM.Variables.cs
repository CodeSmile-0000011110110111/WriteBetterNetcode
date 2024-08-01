// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		public class Variables
		{
			private readonly Dictionary<String, Variable> m_Variables = new();

			public Variable this[String variableName]
			{
				get
				{
					if (m_Variables.TryGetValue(variableName, out var variable))
						return variable;

					// create a new untyped variable instead
					variable = new Variable();
					m_Variables.Add(variableName, variable);
					return variable;
				}
			}

			internal Variables() {} // forbidden default ctor

			public Variable DefineBool(String name, Boolean value = false)
			{
				ThrowIfVariableNameAlreadyExists(name);

				var variable = Variable.Bool(value);
				m_Variables.Add(name, variable);
				return variable;
			}

			public Variable DefineFloat(String name, Single value = 0f)
			{
				ThrowIfVariableNameAlreadyExists(name);

				var variable = Variable.Float(value);
				m_Variables.Add(name, variable);
				return variable;
			}

			public Variable DefineInt(String name, Int32 value = 0)
			{
				ThrowIfVariableNameAlreadyExists(name);

				var variable = Variable.Int(value);
				m_Variables.Add(name, variable);
				return variable;
			}

			public void Clear() => m_Variables.Clear();

			internal String FindName(Variable variable) => m_Variables.FirstOrDefault(kvp => kvp.Value == variable).Key;

			private void ThrowIfVariableNameAlreadyExists(String name)
			{
				if (m_Variables.ContainsKey(name))
					throw new ArgumentException($"Variable named '{name}' already exists");
			}
		}
	}
}
