// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Services.Relay.Models;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services
{
	[Serializable]
	public struct RelayConfig
	{
		public Boolean UseRelayService;
		[Range(1, 100)]
		public Byte MaxConnections;
		public String Region;
		public String JoinCode { get; set; }

		public Allocation HostAllocation { get; private set; }
		public JoinAllocation JoinAllocation { get; private set; }

		public Boolean IsReady => HostAllocation != null || JoinAllocation != null;

		public void SetHostAllocation(Allocation alloc, String joinCode)
		{
			HostAllocation = alloc;
			JoinCode = joinCode;
			// TODO: invoke event here
		}

		// TODO: invoke event here
		public void SetJoinAllocation(JoinAllocation alloc) => JoinAllocation = alloc;

		public void ClearAllocationData()
		{
			HostAllocation = null;
			JoinAllocation = null;
			// TODO: invoke event here
		}

		public override String ToString() =>
			$"{nameof(RelayConfig)}(Relay={UseRelayService}, MaxConnections={MaxConnections}, Region={Region}, " +
			$"JoinCode={JoinCode}, HostAllocation={HostAllocation}, JoinAllocation={JoinAllocation})";
	}
}
