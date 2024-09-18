// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Relay.Actions
{
	/// <summary>
	/// Clears the allocations from the RelayConfig variable.
	/// </summary>
	public sealed class RelayClearAllocationData : IAction
	{
		private readonly Var<RelayConfig> m_RelayConfigVar;

		private RelayClearAllocationData() {}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="relayConfigVar"></param>
		public RelayClearAllocationData(Var<RelayConfig> relayConfigVar) => m_RelayConfigVar = relayConfigVar;

		public void Execute(FSM sm)
		{
			var config = m_RelayConfigVar.Value;
			config.ClearAllocationData();
			m_RelayConfigVar.Value = config;
		}
	}
}
