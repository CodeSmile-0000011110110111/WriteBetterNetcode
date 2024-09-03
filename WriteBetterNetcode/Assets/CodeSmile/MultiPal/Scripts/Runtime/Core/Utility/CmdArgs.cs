// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Utility
{
	public static class CmdArgs
	{
		private const NumberStyles IntStyle = NumberStyles.Integer | NumberStyles.AllowThousands;
		private const NumberStyles FloatStyle = NumberStyles.Float | NumberStyles.AllowThousands;
		private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

		private static Dictionary<String, String> s_Args;
		private static IDictionary<String, String> Args => s_Args != null ? s_Args : s_Args = ParseCmdLineArgs();

		public static void Log()
		{
			var sb = new StringBuilder("CmdArgs: ");
			var cmdArgs = Environment.GetCommandLineArgs();
			foreach (var arg in cmdArgs)
			{
				sb.Append(arg.ToLower());
				sb.Append(" ");
			}

			Debug.Log(sb.ToString());
		}

		public static Boolean Exists(String key) => Args.ContainsKey(key.ToLower());

		public static String GetString(String key, String defaultValue = null) =>
			Args.TryGetValue(key.ToLower(), out var val) ? val : defaultValue;

		public static Boolean GetBool(String key, Boolean defaultValue = default) =>
			Args.TryGetValue(key.ToLower(), out var str)
				? TryParseBool(str, defaultValue)
				: defaultValue;

		public static Int32 GetInt(String key, Int32 defaultValue = default) => Args.TryGetValue(key.ToLower(), out var str)
			? TryParseInt(str, defaultValue)
			: defaultValue;

		public static Single GetFloat(String key, Single defaultValue = default) => Args.TryGetValue(key.ToLower(), out var str)
			? TryParseFloat(str, defaultValue)
			: defaultValue;

		private static Boolean TryParseBool(String str, Boolean defaultValue) =>
			Boolean.TryParse(str, out var val) ? val : defaultValue;

		private static Single TryParseFloat(String str, Single defaultValue) =>
			Single.TryParse(str, FloatStyle, Culture, out var val)
				? val
				: defaultValue;

		private static Int32 TryParseInt(String str, Int32 defaultValue) =>
			Int32.TryParse(str, IntStyle, Culture, out var val) ? val : defaultValue;

		private static Dictionary<String, String> ParseCmdLineArgs()
		{
			var parsedArgs = new Dictionary<String, String>();
			var cmdArgs = Environment.GetCommandLineArgs();
			var argCount = cmdArgs.Length;

			// make all lowercase
			for (var i = 0; i < argCount; ++i)
				cmdArgs[i] = cmdArgs[i].ToLower();

			// parse
			for (var i = 0; i < argCount; ++i)
			{
				var argKey = cmdArgs[i];
				if (argKey.StartsWith("-"))
				{
					// last argKey may not have a value
					var argValue = i < argCount - 1 ? cmdArgs[i + 1] : null;

					// don't use the next argKey as the value
					argValue = argValue?.StartsWith("-") ?? false ? null : argValue;

					// remove the leading argKey dash
					argKey = argKey.Substring(1, argKey.Length - 1);

					if (parsedArgs.ContainsKey(argKey) == false)
						parsedArgs.Add(argKey, argValue);
					else if (Application.isEditor == false)
						Debug.LogWarning($"Duplicate argument: -{argKey}");
				}
			}

			return parsedArgs;
		}
	}
}
