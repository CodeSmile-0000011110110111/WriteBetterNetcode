// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode.CommandLine
{
	[Serializable]
	public class CmdArgRelay : CmdArgBase, ICmdArgImpl
	{
		public void Reset()
		{
			Description = "Enables relay service. Client supplies the join code as parameter.";
			Argument = "-relay";
			Parameters = new[] { "{joincode}" };
		}

		public void OnValidate()
		{
			if (Argument != null)
				Argument = Argument.ToLower();
		}

		public void Process()
		{
			if (Args.TryGetValue(Argument, out var joinCode))
			{
				NetcodeUtility.UseRelayService = true;

				// clients provide relay join code as argument
				if (String.IsNullOrWhiteSpace(joinCode))
				{
					Debug.Log("Relay service enabled.");
					NetcodeUtility.RelayJoinCode = String.Empty;
				}
				else
				{
					Debug.Log($"Connecting through Relay with join code: {joinCode}");
					NetcodeUtility.RelayJoinCode = joinCode;
				}
			}
		}
	}
}
