// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;

namespace CodeSmile.Netcode.CommandLine
{
	public interface ICmdArgImpl
	{
		void Reset();
		void Process();
		void OnValidate();
	}

	[Serializable]
	public abstract class CmdArgBase
	{
		public String Description = "<no description>";
		public String Argument = "-?";
		public String[] Parameters = new String[0];


		private static Dictionary<String, String> s_Args;

		public static IDictionary<String, String> Args =>
			s_Args != null ? s_Args : s_Args = ExtractArguments();

		private static Dictionary<String, String> ExtractArguments()
		{
			var validArgs = new Dictionary<String, String>();

			var args = Environment.GetCommandLineArgs();
			for (var i = 0; i < args.Length; ++i)
			{
				var arg = args[i].ToLower();
				if (arg.StartsWith("-"))
				{
					var argName = i < args.Length - 1 ? args[i + 1].ToLower() : null;
					argName = argName?.StartsWith("-") ?? false ? null : argName;

					validArgs.Add(arg, argName);
				}
			}

			return validArgs;
		}
	}
}
