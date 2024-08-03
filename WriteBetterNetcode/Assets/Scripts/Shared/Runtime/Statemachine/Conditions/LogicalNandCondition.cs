// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public class LogicalNandCondition : FSM.ICondition
	{
		private readonly FSM.ICondition m_AndCondition;

		private LogicalNandCondition() {} // forbidden default ctor

		internal LogicalNandCondition(params FSM.ICondition[] nandConditions) =>
			m_AndCondition = new LogicalAndCondition(nandConditions);

		public Boolean IsSatisfied(FSM sm) => !m_AndCondition.IsSatisfied(sm);

		public String ToDebugString(FSM sm) => $"N{m_AndCondition.ToDebugString(sm)}";

		public void OnStart(FSM sm) => m_AndCondition.OnStart(sm);
		public void OnStop(FSM sm) => m_AndCondition.OnStop(sm);
		public void OnEnterState(FSM sm) => m_AndCondition.OnEnterState(sm);
		public void OnExitState(FSM sm) => m_AndCondition.OnExitState(sm);
	}
}
