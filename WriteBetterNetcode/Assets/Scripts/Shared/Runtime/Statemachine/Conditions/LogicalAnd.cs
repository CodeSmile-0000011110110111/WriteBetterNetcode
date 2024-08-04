﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Conditions
{
	public sealed class LogicalAnd : FSM.ICondition
	{
		private readonly FSM.ICondition[] m_InnerConditions;
		internal FSM.ICondition[] InnerConditions => m_InnerConditions;

		private static void VerifyParameters(FSM.ICondition[] andConditions)
		{
#if DEBUG || DEVELOPMENT_BUILD
			if (andConditions == null)
				throw new ArgumentNullException(nameof(andConditions));
			if (andConditions.Length < 2)
				throw new ArgumentException("AND: at least two conditions required!");

			foreach (var cond in andConditions)
			{
				if (cond == null)
					throw new ArgumentNullException($"{nameof(andConditions)} contains null");
			}
#endif
		}

		private LogicalAnd() {} // forbidden default ctor

		internal LogicalAnd(params FSM.ICondition[] andConditions)
		{
			VerifyParameters(andConditions);
			m_InnerConditions = andConditions;
		}

		public Boolean IsSatisfied(FSM sm)
		{
			foreach (var condition in InnerConditions)
			{
				if (condition.IsSatisfied(sm) == false)
					return false;
			}
			return true;
		}

		public String ToDebugString(FSM sm)
		{
			var sb = new StringBuilder("AND(");
			for (var i = 0; i < m_InnerConditions.Length; i++)
			{
				if (i > 0)
					sb.Append(", ");

				sb.Append(m_InnerConditions[i].ToDebugString(sm));
			}
			sb.Append(")");
			return sb.ToString();
		}

		public void OnStart(FSM sm)
		{
			foreach (var condition in InnerConditions)
				condition.OnStart(sm);
		}

		public void OnStop(FSM sm)
		{
			foreach (var condition in InnerConditions)
				condition.OnStop(sm);
		}

		public void OnEnterState(FSM sm)
		{
			foreach (var condition in InnerConditions)
				condition.OnEnterState(sm);
		}

		public void OnExitState(FSM sm)
		{
			foreach (var condition in InnerConditions)
				condition.OnExitState(sm);
		}
	}
}