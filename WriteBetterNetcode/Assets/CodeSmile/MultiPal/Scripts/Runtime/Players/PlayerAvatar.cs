// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Animation;
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
		private IAnimationDataProvider m_AnimationDataProvider;
		private KyleAnimatorParams m_AnimatorParams;

		public Byte PreviousIndex => (Byte)(m_Player.AvatarIndex == 0 ? m_AvatarPrefabs.Count - 1 : m_Player.AvatarIndex - 1);
		public Byte NextIndex => (Byte)(m_Player.AvatarIndex == m_AvatarPrefabs.Count - 1 ? 0 : m_Player.AvatarIndex + 1);

		public void OnPlayerSpawn(Int32 playerIndex) {}
		public void OnPlayerDespawn(Int32 playerIndex) {}

		private void Awake()
		{
			m_Player = GetComponent<Player>();
			m_AnimationDataProvider = GetComponent<IAnimationDataProvider>();
		}

		private void LateUpdate()
		{
			if (m_AnimationDataProvider != null && m_AnimatorParams != null)
			{
				var animationData = m_AnimationDataProvider.AnimationData;
				if (animationData != null)
					m_AnimatorParams.Apply(animationData);
			}
		}

		internal void SetAvatar(Byte avatarIndex)
		{
			var prefab = m_AvatarPrefabs[avatarIndex];
			if (prefab != null)
			{
				if (m_AvatarInstance != null)
					Destroy(m_AvatarInstance);

				m_AvatarInstance = Instantiate(prefab, transform);
				m_AnimatorParams = m_AvatarInstance.GetComponentInChildren<KyleAnimatorParams>();
			}
		}
	}
}
