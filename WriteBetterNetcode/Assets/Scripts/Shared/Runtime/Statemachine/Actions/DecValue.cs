// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Actions
{
	public class DecValue : FSM.IAction
	{
		private readonly FSM.IntVar m_Variable;

		private DecValue() {} // forbidden default ctor

		private DecValue(FSM.IntVar variable) => m_Variable = variable;

		public void Execute(FSM sm) => m_Variable.Value--;

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)}--";
	}
}
