// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Animation;
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
		private PlayerClient m_ClientSide;
		private GameObject m_AvatarInstance;
		private AnimatorParametersBase m_AnimatorParameters;

		public Byte PreviousIndex => (Byte)(m_Player.AvatarIndex == 0 ? m_AvatarPrefabs.Count - 1 : m_Player.AvatarIndex - 1);
		public Byte NextIndex => (Byte)(m_Player.AvatarIndex == m_AvatarPrefabs.Count - 1 ? 0 : m_Player.AvatarIndex + 1);

		public void OnPlayerSpawn(Int32 playerIndex, Boolean isOwner) => SetAvatar(playerIndex, m_Player.AvatarIndex, isOwner);

		public void OnPlayerDespawn(Int32 playerIndex, Boolean isOwner) {}

		private void Awake()
		{
			m_Player = GetComponent<Player>();
			m_ClientSide = GetComponent<PlayerClient>();
		}

		private void LateUpdate()
		{
			if (m_AnimatorParameters != null)
				m_ClientSide.SyncAnimatorParameters(m_AnimatorParameters);
		}

		internal void SetAvatar(Int32 playerIndex, Byte avatarIndex, Boolean isOwner)
		{
			// ignore pre-spawn AvatarIndex value change events, it'll get spawned in OnPlayerSpawn
			if (playerIndex < 0)
				return;

			var prefab = m_AvatarPrefabs[avatarIndex];
			if (prefab != null)
			{
				if (m_AvatarInstance != null)
				{
					Destroy(m_AvatarInstance);
					m_AnimatorParameters = null;
				}

				m_AvatarInstance = Instantiate(prefab, transform);

				if (isOwner)
				{
					if (m_AvatarInstance.TryGetComponent<IAnimatorController>(out var animCtrl))
						m_AnimatorParameters = animCtrl.GetAnimatorParameters(m_Player.PlayerIndex);
				}
			}
		}

		internal void OnAvatarIndexChanged(Byte _, Byte avatarIndex) =>
			SetAvatar(m_Player.PlayerIndex, avatarIndex, m_Player.IsOwner);

		public void ReceiveAnimatorParameters(AnimatorParametersBase animatorParameters)
		{
			Debug.Log($"received animator parameters: {animatorParameters}");
			if (m_AvatarInstance != null)
			{
				if (m_AvatarInstance.TryGetComponent<IAnimatorController>(out var animCtrl))
					animCtrl.SetAnimatorParameters(m_Player.PlayerIndex, animatorParameters);
			}

			m_AnimatorParameters = animatorParameters;
		}
	}
}
