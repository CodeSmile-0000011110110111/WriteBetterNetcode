// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.MultiPal
{
	public static class AddDefaultMppmTags
	{
		[InitializeOnLoadMethod]
		private static void InitOnLoad() => TryAddMppmTags();

		private static void TryAddMppmTags()
		{
			const BindingFlags StaticBindingFlags = BindingFlags.Public | BindingFlags.Static;
			const String FullyQualifiedMppmName =
				"Unity.Multiplayer.Playmode.Workflow.Editor.MultiplayerPlaymode, Unity.Multiplayer.Playmode.Workflow.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

			var mppmType = Type.GetType(FullyQualifiedMppmName);
			var getPlayerTags = mppmType?.GetProperty("PlayerTags", StaticBindingFlags)?.GetGetMethod();
			var playerTags = getPlayerTags?.Invoke(null, null);
			var addPlayerTag = playerTags?.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);

			// this won't duplicate tags so its safe to not check for existing tags
			addPlayerTag?.Invoke(playerTags, new Object[] { "Server", null });
			addPlayerTag?.Invoke(playerTags, new Object[] { "Host", null });
			addPlayerTag?.Invoke(playerTags, new Object[] { "Client", null });
		}
	}
}
