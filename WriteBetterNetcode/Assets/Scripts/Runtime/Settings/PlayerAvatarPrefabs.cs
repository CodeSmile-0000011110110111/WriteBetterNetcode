// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Settings
{
	[CreateAssetMenu(fileName = "Player Avatars",
		menuName = "CodeSmile/Player Avatars", order = 0)]
	public sealed class PlayerAvatarPrefabs : ScriptableObject
	{
		[SerializeField] private List<GameObject> m_Prefabs = new();

		public GameObject this[Byte avatarIndex] =>
			avatarIndex >= 0 && avatarIndex < m_Prefabs.Count ? m_Prefabs[avatarIndex] : null;
	}
}
