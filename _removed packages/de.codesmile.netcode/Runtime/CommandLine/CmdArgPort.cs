// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Netcode.Extensions;
using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.CommandLine
{
	[Serializable]
	public class CmdArgPort : CmdArgBase, ICmdArgImpl
	{
		public void Reset()
		{
			Description = "The UDP port to connect through.";
			Argument = "-port";
			Parameters = new[] { "7777" };
		}

		public void OnValidate()
		{
			if (Argument != null)
				Argument = Argument.ToLower();
		}

		public void Process()
		{
			if (Args.TryGetValue(Argument, out var port))
			{
				if (String.IsNullOrWhiteSpace(port) == false)
				{
					Debug.Log($"Using port: {port}");
					var transport = NetworkManager.Singleton.GetTransport();

					UInt16.TryParse(port, out var portNumber);
					transport.ConnectionData.Port = portNumber;
				}
			}
		}
	}
}
