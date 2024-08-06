// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	[Serializable]
	public struct RelayConfig
	{
		public Boolean UseRelayService;
		public String RelayJoinCode;

		public override String ToString() => $"{nameof(RelayConfig)}(Relay={UseRelayService}, JoinCode={RelayJoinCode})";
	}
}
