// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Relay.Conditions
{
	public class IsRelayEnabled : ICondition
	{
		private readonly Var<RelayConfig> m_RelayConfig;

		private IsRelayEnabled() {} // forbidden

		public IsRelayEnabled(Var<RelayConfig> relayConfig) => m_RelayConfig = relayConfig;

		public Boolean IsSatisfied(FSM sm) => m_RelayConfig.Value.UseRelayService;
	}
}
