// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		public sealed class LogicalNotCondition : ICondition
		{
			private readonly ICondition m_InnerCondition;
			internal ICondition InnerCondition => m_InnerCondition;

			private static void VerifyParameter(ICondition notCondition)
			{
#if DEBUG || DEVELOPMENT_BUILD
				if (notCondition == null)
					throw new ArgumentNullException(nameof(notCondition));
#endif
			}

			private LogicalNotCondition() {} // forbidden default ctor

			internal LogicalNotCondition(ICondition notCondition)
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
}
