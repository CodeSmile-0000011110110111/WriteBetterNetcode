// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Utility;
using System;
using Unity.Services.Relay.Models;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	/// <summary>
	/// DTO that provices Relay configuration options for NetworkManager Transport setup.
	/// </summary>
	[Serializable]
	public struct RelayConfig
	{
		/// <summary>
		/// Relay is limited to 100 connections according to the documentation (09/2024).
		/// </summary>
		public const Int32 MaxRelayConnections = 100;

		/// <summary>
		/// Set to true to enable use of relay for this session. If false, none of the other parameter matter.
		/// </summary>
		public Boolean UseRelay;

		/// <summary>
		/// Maximum number of connections (clients) per session.
		/// </summary>
		[Range(0, MaxRelayConnections)] public Byte MaxConnections;

		/// <summary>
		/// Optional. The region string (see Relay manual) to select a specific Relay server location.
		/// If null or empty, will use QoS to determine the Relay server location based on host's location and latency.
		/// </summary>
		public String Region;

		private String m_JoinCode;

		/// <summary>
		/// The relay join code. Server/Host will assign the join code for read-out by GUI etc. after Relay has started.
		/// Clients will have to assign a join code before attempting to connect.
		/// </summary>
		public String JoinCode { readonly get => m_JoinCode; set => m_JoinCode = value?.ToUpper(); }

		/// <summary>
		/// The HostAllocation instance is set by the server only.
		/// </summary>
		public Allocation HostAllocation { get; private set; }
		/// <summary>
		/// The JoinAllocation instance is set by the client only.
		/// </summary>
		public JoinAllocation JoinAllocation { get; private set; }

		/// <summary>
		/// Is true if either a host or join allocation is non-null. Determines if we're good to go.
		/// </summary>
		public Boolean HasAllocation => HostAllocation != null || JoinAllocation != null;

		/// <summary>
		/// Creates a RelayConfig from command line arguments. Omitted parameters will use default values.
		/// </summary>
		/// <returns></returns>
		public static RelayConfig FromCmdArgs() => new()
		{
			UseRelay = CmdArgs.GetBool(nameof(UseRelay)),
			Region = CmdArgs.GetString(nameof(Region)),
			JoinCode = CmdArgs.GetString(nameof(JoinCode)),
			MaxConnections = (Byte)Mathf.Clamp(
				CmdArgs.GetInt(nameof(MaxConnections)),
				0, MaxRelayConnections),
		};

		/// <summary>
		/// Server: assign both the received allocation and the received join code.
		/// </summary>
		/// <param name="alloc"></param>
		/// <param name="joinCode"></param>
		public void SetHostAllocation(Allocation alloc, String joinCode)
		{
			HostAllocation = alloc;
			JoinAllocation = null;
			JoinCode = joinCode;
		}

		/// <summary>
		/// Client: assign the received allocation after joining.
		/// </summary>
		/// <param name="alloc"></param>
		public void SetJoinAllocation(JoinAllocation alloc)
		{
			HostAllocation = null;
			JoinAllocation = alloc;
		}

		/// <summary>
		/// Set any existing allocations to null.
		/// </summary>
		public void ClearAllocationData()
		{
			HostAllocation = null;
			JoinAllocation = null;
		}

		public override String ToString() => $"{nameof(RelayConfig)}(" +
		                                     $"{nameof(UseRelay)}={UseRelay}, " +
		                                     $"{nameof(MaxConnections)}={MaxConnections}, " +
		                                     $"{nameof(Region)}={Region}, " +
		                                     $"{nameof(JoinCode)}={JoinCode})";
	}
}
