﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Statemachine.Actions
{
	public sealed class CompoundAction : IAction
	{
		private readonly String m_Name;
		private readonly IAction[] m_Actions;

		private CompoundAction() {}

		public CompoundAction(params IAction[] actions)
			: this(null, actions) {}

		public CompoundAction(String name, params IAction[] actions)
		{
			if (actions == null || actions.Length == 0)
				throw new ArgumentException("actions null or empty");

			m_Name = name;
			m_Actions = actions;
		}

		public async void Execute(FSM sm) => await FSM.Transition.ExecuteActions(sm, null, m_Actions, sm.ActiveState.Logging);

		public String ToDebugString(FSM sm)
		{
			if (String.IsNullOrWhiteSpace(m_Name) == false)
				return m_Name;

			var sb = new StringBuilder();
			foreach (var action in m_Actions)
				sb.AppendLine(action.ToDebugString(sm));

			return sb.ToString();
		}
	}
}