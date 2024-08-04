// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Actions
{
	public class Decrement : FSM.IAction
	{
		private readonly FSM.IntVariable m_Variable;

		private Decrement() {} // forbidden default ctor

		private Decrement(FSM.IntVariable variable) => m_Variable = variable;

		public void Execute(FSM sm) => m_Variable.Value--;

		public String ToDebugString(FSM sm) => $"{sm.GetDebugVarName(m_Variable)}--";
	}
}
