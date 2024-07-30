// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Netcode.Extensions;
using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.CommandLine
{
	[Serializable]
	public class CmdArgAddress : CmdArgBase, ICmdArgImpl
	{
		public void Reset()
		{
			Debug.Log("RESET for CmdArgAddress ");

			Description = "The IPv4 or IPv6 address to connect to.";
			Argument = "-address";
			Parameters = new[] { "{0.0.0.0} or {::}" };
		}

		public void OnValidate()
		{
			if (Argument != null)
				Argument = Argument.ToLower();
		}

		public void Process()
		{
			if (Args.TryGetValue(Argument, out var address))
			{
				if (String.IsNullOrWhiteSpace(address) == false)
				{
					Debug.Log($"Using address: {address}");
					var transport = NetworkManager.Singleton.GetTransport();
					transport.ConnectionData.Address = address;
				}
			}
		}
	}
}
