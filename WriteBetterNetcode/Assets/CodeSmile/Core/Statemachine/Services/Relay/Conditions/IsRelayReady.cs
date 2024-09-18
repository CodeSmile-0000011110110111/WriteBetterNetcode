// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Netcode;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Relay.Conditions
{
	/// <summary>
	/// Is true if RelayConfig has an allocation, be it a Host or Join allocation.
	/// </summary>
	public sealed class IsRelayReady : ICondition
	{
		private readonly Var<RelayConfig> m_RelayConfigVar;
		private IsRelayReady() {}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="relayConfigVar"></param>
		public IsRelayReady(Var<RelayConfig> relayConfigVar) => m_RelayConfigVar = relayConfigVar;

		public Boolean IsSatisfied(FSM sm) => m_RelayConfigVar.Value.HasAllocation;
	}
}
