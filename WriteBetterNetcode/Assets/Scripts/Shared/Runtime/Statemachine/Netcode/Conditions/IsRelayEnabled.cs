// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	public class IsRelayEnabled : ICondition
	{
		private readonly StructVar<RelayConfig> m_RelayConfig;

		private IsRelayEnabled() {} // forbidden

		public IsRelayEnabled(StructVar<RelayConfig> relayConfig) => m_RelayConfig = relayConfig;

		public Boolean IsSatisfied(FSM sm) => m_RelayConfig.Value.UseRelayService;
	}
}
