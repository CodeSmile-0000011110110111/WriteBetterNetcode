﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Relay.Actions
{
	public sealed class RelayClearAllocationData : IAction
	{
		private readonly Var<RelayConfig> m_RelayConfigVar;

		private RelayClearAllocationData() {}

		public RelayClearAllocationData(Var<RelayConfig> relayConfigVar) => m_RelayConfigVar = relayConfigVar;

		public void Execute(FSM sm)
		{
			var config = m_RelayConfigVar.Value;
			config.ClearAllocationData();
			m_RelayConfigVar.Value = config;
		}
	}
}
