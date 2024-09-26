// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.MultiPal.Editor
{
	public static class AddDefaultMppmTags
	{
		private static readonly String FirstTimeKey = "CodeSmileEditor.MultiPal." +
		                                              nameof(AddDefaultMppmTags) + PlayerSettings.productGUID +
		                                              ".FirstTimeImport_3";

		private static Boolean IsFirstTime
		{
			get => EditorPrefs.GetBool(FirstTimeKey, true);
			set => EditorPrefs.SetBool(FirstTimeKey, value);
		}

		[InitializeOnLoadMethod]
		private static void InitOnLoad()
		{
			if (IsFirstTime)
			{
				EditorApplication.delayCall += () =>
				{
					IsFirstTime = false;
					TryAddMppmTags();
				};
			}
		}

		private static void TryAddMppmTags()
		{
			const BindingFlags StaticBindingFlags = BindingFlags.Public | BindingFlags.Static;
			const BindingFlags InstanceBindingFlags = BindingFlags.Public | BindingFlags.Instance;
			const String FullyQualifiedMppmName = "Unity.Multiplayer.Playmode.Workflow.Editor.MultiplayerPlaymode, " +
			                                      "Unity.Multiplayer.Playmode.Workflow.Editor, " +
			                                      "Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

			var mppmType = Type.GetType(FullyQualifiedMppmName);
			var getPlayerTags = mppmType?.GetProperty("PlayerTags", StaticBindingFlags)?.GetGetMethod();
			var playerTags = getPlayerTags?.Invoke(null, null);
			var addPlayerTag = playerTags?.GetType().GetMethod("Add", InstanceBindingFlags);

			// this won't duplicate tags so it's safe to not check for existing tags
			addPlayerTag?.Invoke(playerTags, new Object[] { "Server", null });
			addPlayerTag?.Invoke(playerTags, new Object[] { "Host", null });
			addPlayerTag?.Invoke(playerTags, new Object[] { "Client", null });

			// try again later, import may not have completed yet
			if (getPlayerTags == null || playerTags == null || addPlayerTag == null)
				IsFirstTime = true;
		}
	}
}
