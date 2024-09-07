// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Interfaces;
using CodeSmile.MultiPal.Settings;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players
{
	[DisallowMultipleComponent]
	internal sealed class PlayerAvatar : MonoBehaviour, IPlayerComponent
	{
		[SerializeField] private PlayerAvatarPrefabs m_AvatarPrefabs;

		private Player m_Player;
		private GameObject m_AvatarInstance;

		public Byte PreviousIndex => (Byte)(m_Player.AvatarIndex == 0 ? m_AvatarPrefabs.Count - 1 : m_Player.AvatarIndex - 1);
		public Byte NextIndex => (Byte)(m_Player.AvatarIndex == m_AvatarPrefabs.Count - 1 ? 0 : m_Player.AvatarIndex + 1);

		public void OnPlayerSpawn(Int32 playerIndex, Boolean isOwner) => SetAvatar(playerIndex, m_Player.AvatarIndex, isOwner);

		public void OnPlayerDespawn(Int32 playerIndex, Boolean isOwner) {}

		private void Awake() => m_Player = GetComponent<Player>();

		internal void SetAvatar(Int32 playerIndex, Byte avatarIndex, Boolean isOwner)
		{
			// ignore pre-spawn AvatarIndex value change events, it'll get spawned in OnPlayerSpawn
			if (playerIndex < 0)
				return;

			var prefab = m_AvatarPrefabs[avatarIndex];
			if (prefab != null)
			{
				if (m_AvatarInstance != null)
					Destroy(m_AvatarInstance);

				m_AvatarInstance = Instantiate(prefab, transform);

				if (isOwner)
				{
					if (m_AvatarInstance.TryGetComponent<IAnimatorController>(out var animCtrl))
						animCtrl.OnAssignAnimationData(m_Player.PlayerIndex);
				}
			}
		}

		internal void OnAvatarIndexChanged(Byte _, Byte avatarIndex) => SetAvatar(m_Player.PlayerIndex, avatarIndex, m_Player.IsOwner);
	}
}
