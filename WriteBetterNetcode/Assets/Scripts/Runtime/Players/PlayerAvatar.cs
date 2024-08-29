// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Settings;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	internal sealed class PlayerAvatar : MonoBehaviour, IPlayerComponent
	{
		[SerializeField] private PlayerAvatarPrefabs m_AvatarPrefabs;

		private GameObject m_AvatarInstance;

		public void OnPlayerSpawn(Int32 playerIndex) {}
		public void OnPlayerDespawn(Int32 playerIndex) {}

		internal void SetAvatar(Byte avatarIndex)
		{
			var prefab = m_AvatarPrefabs[avatarIndex];
			if (prefab != null)
			{
				if (m_AvatarInstance != null)
					Destroy(m_AvatarInstance);

				m_AvatarInstance = Instantiate(prefab, transform);
			}
		}
	}
}
