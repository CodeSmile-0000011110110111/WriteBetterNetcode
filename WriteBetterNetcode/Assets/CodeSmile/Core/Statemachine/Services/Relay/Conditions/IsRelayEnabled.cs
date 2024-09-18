// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Netcode;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Relay.Conditions
{
	/// <summary>
	/// Is true if the relay configuration has relay enabled.
	/// </summary>
	public sealed class IsRelayEnabled : ICondition
	{
		private readonly Var<RelayConfig> m_RelayConfig;

		private IsRelayEnabled() {} // forbidden

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="relayConfig"></param>
		public IsRelayEnabled(Var<RelayConfig> relayConfig) => m_RelayConfig = relayConfig;

		public Boolean IsSatisfied(FSM sm) => m_RelayConfig.Value.UseRelay;
	}
}
