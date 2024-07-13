// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeSmileEditor.Netcode
{
	/// <summary>
	/// Creates default set of player tags for Multiplayer PlayMode if the player tags file is missing.
	/// </summary>
	public static class MppmPlayerTagInit
	{
		private const String SessionStateKey_DidCheckPlayerTags = "CodeSmile.MPPM.DidCheckPlayerTags";
		private const String FilePath = "ProjectSettings/VirtualProjectsConfig.json";
		private const String FileContents = @"{
  ""PlayerTags"": [
    ""Client"",
    ""Host"",
    ""Server""
  ]
}";

		[InitializeOnLoadMethod]
		private static void OnLoad()
		{
			var didCheck = SessionState.GetBool(SessionStateKey_DidCheckPlayerTags, false);
			if (didCheck == false)
			{
				SessionState.SetBool(SessionStateKey_DidCheckPlayerTags, true);

				try
				{
					AddPlayerTagsIfEmpty();
				}
				catch (Exception e)
				{
					Debug.LogWarning($"failed to initialize Multiplayer PlayMode player tags: {e}");
					throw;
				}
			}
		}

		private static void AddPlayerTagsIfEmpty()
		{
			var fullPath = Path.GetFullPath($"{Application.dataPath}/../{FilePath}");
			if (File.Exists(fullPath) == false)
			{
				Debug.Log("MPPM: Creating default player tags ...");
				File.WriteAllText(fullPath, FileContents);

				// only script reload updates the list in project settings
				EditorApplication.delayCall += () => EditorUtility.RequestScriptReload();
			}
		}
	}
}
