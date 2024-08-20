// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeSmile.Player
{
	public class LocalPlayerAvatar
	{
		private readonly LocalPlayer m_Player;

		private GameObject m_Active;

		private int m_ActiveIndex;
		public Int32 ActiveIndex => m_ActiveIndex;

		public LocalPlayerAvatar(LocalPlayer player) => m_Player = player;

		public void Select(Int32 avatarIndex)
		{
			if (m_Active != null)
				Object.Destroy(m_Active);

			var prefab = m_Player.AvatarPrefabs.GetPrefab(avatarIndex);
			m_Active = Object.Instantiate(prefab, m_Player.transform);
			m_ActiveIndex = avatarIndex;
		}
	}
}
