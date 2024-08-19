// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Game
{
	[CreateAssetMenu(fileName = "Player Avatars", menuName = "CodeSmile/Player Avatars", order = 0)]
	public sealed class PlayerAvatarPrefabs : ScriptableObject
	{
		[SerializeField] private List<GameObject> m_Prefabs = new();

		public GameObject GetPrefab(Int32 avatarIndex)
		{
			avatarIndex = Mathf.Clamp(avatarIndex, 0, m_Prefabs.Count - 1);
			return m_Prefabs[avatarIndex];
		}
	}
}
