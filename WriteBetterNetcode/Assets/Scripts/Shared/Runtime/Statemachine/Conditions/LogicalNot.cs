// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Conditions
{
	public sealed class LogicalNot : FSM.ICondition
	{
		private readonly FSM.ICondition m_InnerCondition;
		internal FSM.ICondition InnerCondition => m_InnerCondition;

		private static void VerifyParameter(FSM.ICondition notCondition)
		{
#if DEBUG || DEVELOPMENT_BUILD
			if (notCondition == null)
				throw new ArgumentNullException(nameof(notCondition));
#endif
		}

		private LogicalNot() {} // forbidden default ctor

		internal LogicalNot(FSM.ICondition notCondition)
		{
			VerifyParameter(notCondition);
			m_InnerCondition = notCondition;
		}

		public Boolean IsSatisfied(FSM sm) => !InnerCondition.IsSatisfied(sm);

		public String ToDebugString(FSM sm) => $"NOT({InnerCondition.ToDebugString(sm)})";

		public void OnStart(FSM sm) => m_InnerCondition.OnStart(sm);
		public void OnStop(FSM sm) => m_InnerCondition.OnStop(sm);
		public void OnEnterState(FSM sm) => m_InnerCondition.OnEnterState(sm);
		public void OnExitState(FSM sm) => m_InnerCondition.OnExitState(sm);
	}
}
