// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Netcode;
using System.Threading.Tasks;
using Unity.Services.Relay;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Relay.Actions
{
	public class RelayCreateOrJoinAllocation : IAsyncAction
	{
		private readonly Var<RelayConfig> m_RelayConfigVar;
		private readonly Var<NetcodeConfig> m_NetcodeConfigVar;

		private RelayCreateOrJoinAllocation() {}

		public RelayCreateOrJoinAllocation(Var<NetcodeConfig> netcodeConfigVar, Var<RelayConfig> relayConfigVar)
		{
			m_NetcodeConfigVar = netcodeConfigVar;
			m_RelayConfigVar = relayConfigVar;
		}

		public async Task ExecuteAsync(FSM sm)
		{
			ClearRelayAllocationVar();

			var role = m_NetcodeConfigVar.Value.Role;
			var relayConfig = m_RelayConfigVar.Value;
			var relay = RelayService.Instance;

			if (role == NetcodeRole.Server || role == NetcodeRole.Host)
			{
				var allocation = await relay.CreateAllocationAsync(relayConfig.MaxConnections, relayConfig.Region);
				var joinCode = await relay.GetJoinCodeAsync(allocation.AllocationId);
				relayConfig.SetHostAllocation(allocation, joinCode);
			}
			else
			{
				var joinAlloc = await relay.JoinAllocationAsync(relayConfig.JoinCode);
				relayConfig.SetJoinAllocation(joinAlloc);
			}

			// write back
			m_RelayConfigVar.Value = relayConfig;
		}

		private void ClearRelayAllocationVar()
		{
			var config = m_RelayConfigVar.Value;
			config.ClearAllocationData();
			m_RelayConfigVar.Value = config;
		}
	}
}
