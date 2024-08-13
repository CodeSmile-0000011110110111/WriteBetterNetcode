// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Core.Utility;
using System;
using Unity.Services.Relay.Models;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Statemachine.Netcode
{
	[Serializable]
	public struct RelayConfig
	{
		public const Int32 MinRelayConnections = 1;
		public const Int32 MaxRelayConnections = 100;

		public Boolean UseRelay;
		[Range(MinRelayConnections, MaxRelayConnections)] public Byte MaxConnections;
		public String Region;
		public String JoinCode { get; set; }

		public Allocation HostAllocation { get; private set; }
		public JoinAllocation JoinAllocation { get; private set; }

		public Boolean HasAllocation => HostAllocation != null || JoinAllocation != null;

		public static RelayConfig FromCmdArgs() => new()
		{
			UseRelay = CmdArgs.GetBool(nameof(UseRelay)),
			Region = CmdArgs.GetString(nameof(Region)),
			JoinCode = CmdArgs.GetString(nameof(JoinCode)),
			MaxConnections = (Byte)Mathf.Clamp(CmdArgs.GetInt(nameof(MaxConnections)),
				MinRelayConnections, MaxRelayConnections),
		};

		public void SetHostAllocation(Allocation alloc, String joinCode)
		{
			HostAllocation = alloc;
			JoinAllocation = null;
			JoinCode = joinCode;
			// TODO: invoke event here
		}

		// TODO: invoke event here
		public void SetJoinAllocation(JoinAllocation alloc)
		{
			HostAllocation = null;
			JoinAllocation = alloc;
		}

		public void ClearAllocationData()
		{
			HostAllocation = null;
			JoinAllocation = null;
			// TODO: invoke event here?
		}

		public override String ToString() => $"{nameof(RelayConfig)}(" +
		                                     $"{nameof(UseRelay)}={UseRelay}, " +
		                                     $"{nameof(MaxConnections)}={MaxConnections}, " +
		                                     $"{nameof(Region)}={Region}, " +
		                                     $"{nameof(JoinCode)}={JoinCode})";
	}
}
