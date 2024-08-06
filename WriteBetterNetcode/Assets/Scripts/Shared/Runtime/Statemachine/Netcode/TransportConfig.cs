// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	[Serializable]
	public struct TransportConfig
	{
		public String Address;
		public UInt16 Port;
		public String ServerListenAddress;
		public Boolean UseEncryption;
		public Boolean UseWebSockets;

		public override String ToString() =>
			$"{nameof(TransportConfig)}(Address={Address}:{Port}, ListenAddress={ServerListenAddress}, " +
			$"UseWebSockets={UseWebSockets}, UseEncryption={UseEncryption})";
	}
}
