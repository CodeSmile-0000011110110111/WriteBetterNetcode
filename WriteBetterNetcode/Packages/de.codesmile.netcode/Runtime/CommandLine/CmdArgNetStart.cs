// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;
using CodeSmile.SceneTools;
using System;
using UnityEngine;

namespace CodeSmile.Netcode.CommandLine
{
	[Serializable]
	public class CmdArgNetStart : CmdArgBase, ICmdArgImpl
	{
		public void Reset()
		{
			Description = "Starts the application as either server, host or client.";
			Argument = "-netstart";
			Parameters = new[] { "server", "host", "client" };
		}

		public void OnValidate()
		{
			if (Argument != null)
				Argument = Argument.ToLower();
		}

		public async void Process()
		{
			if (Args.TryGetValue(Argument, out var mode))
			{
				var paramsIndex = 0;
				if (mode.Equals(Parameters[paramsIndex++]))
				{
					Debug.Log("Starting Server ...");
					SceneAutoLoader.DestroyAll(); // server loads scene via NetworkSessionState
					await NetcodeUtility.StartServer();
				}
				else if (mode.Equals(Parameters[paramsIndex++]))
				{
					Debug.Log("Starting Host ...");
					SceneAutoLoader.DestroyAll(); // server loads scene via NetworkSessionState
					await NetcodeUtility.StartHost();
				}
				else if (mode.Equals(Parameters[paramsIndex++]))
				{
					Debug.Log("Starting Client ...");
					SceneAutoLoader.DestroyAll(); // clients auto-load when connected
					await NetcodeUtility.StartClient();
				}
			}
		}
	}
}
