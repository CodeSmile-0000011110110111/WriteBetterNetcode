// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Settings;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	public sealed class PlayerAvatar : MonoBehaviour
	{
		[SerializeField] private PlayerAvatarPrefabs m_AvatarPrefabs;

		private GameObject m_AvatarObject;
		private Player m_Player;
		private PlayerAvatarPrefabs AvatarPrefabs => m_AvatarPrefabs;

		internal void OnAvatarIndexChanged(Byte prevAvatarIndex, Byte avatarIndex)
		{
			Debug.Log($"Avatar Index changed from {prevAvatarIndex} to {avatarIndex}");

			var prefab = AvatarPrefabs.GetPrefab(avatarIndex);
			if (prefab != null)
			{
				if (m_AvatarObject != null)
					Destroy(m_AvatarObject);

				m_AvatarObject = Instantiate(prefab, transform);
			}
		}
	}
}
