// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Animation;
using CodeSmile.MultiPal.Settings;
using System;
using System.Collections;
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

		private void Awake()
		{
			m_Player = GetComponent<Player>();

			var animator = GetComponent<Animator>();
			if (animator != null)
				animator.enabled = false;
		}

		internal void SetAvatar(Int32 playerIndex, Byte avatarIndex, Boolean isOwner)
		{
			// ignore pre-spawn AvatarIndex value change events, it'll get spawned in OnPlayerSpawn
			if (playerIndex < 0)
				return;

			if (avatarIndex >= m_AvatarPrefabs.Count)
			{
				Debug.LogWarning($"AvatarIndex {avatarIndex} out of bounds, using default Avatar");
				avatarIndex = 0;
			}

			var prefab = m_AvatarPrefabs[avatarIndex];
			if (prefab != null)
			{
				if (m_AvatarInstance != null)
					Destroy(m_AvatarInstance);

				m_AvatarInstance = Instantiate(prefab, transform);

				if (m_AvatarInstance.TryGetComponent<IAnimatorController>(out var animCtrl))
					StartCoroutine(DeferAnimatorInit(animCtrl, playerIndex, isOwner));
			}
		}

		private IEnumerator DeferAnimatorInit(IAnimatorController animCtrl, Int32 playerIndex, Boolean isOwner)
		{
			yield return new WaitForEndOfFrame();

			animCtrl.Init(playerIndex, isOwner);
		}

		internal void OnAvatarIndexChanged(Byte _, Byte avatarIndex) =>
			SetAvatar(m_Player.PlayerIndex, avatarIndex, m_Player.IsOwner);
	}
}
