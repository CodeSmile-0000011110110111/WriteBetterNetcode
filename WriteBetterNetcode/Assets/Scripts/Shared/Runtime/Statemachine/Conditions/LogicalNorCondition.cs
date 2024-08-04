// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Conditions
{
	public class LogicalNorCondition : FSM.ICondition
	{
		private readonly FSM.ICondition m_OrCondition;

		private LogicalNorCondition() {} // forbidden default ctor

		internal LogicalNorCondition(params FSM.ICondition[] norConditions) =>
			m_OrCondition = new LogicalOrCondition(norConditions);

		public Boolean IsSatisfied(FSM sm) => !m_OrCondition.IsSatisfied(sm);

		public String ToDebugString(FSM sm) => $"N{m_OrCondition.ToDebugString(sm)}";

		public void OnStart(FSM sm) => m_OrCondition.OnStart(sm);
		public void OnStop(FSM sm) => m_OrCondition.OnStop(sm);
		public void OnEnterState(FSM sm) => m_OrCondition.OnEnterState(sm);
		public void OnExitState(FSM sm) => m_OrCondition.OnExitState(sm);
	}
}
