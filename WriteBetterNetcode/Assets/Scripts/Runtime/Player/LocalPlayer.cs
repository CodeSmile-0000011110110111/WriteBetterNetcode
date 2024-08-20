// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Settings;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeSmile.Player
{
	public sealed class LocalPlayer : NetworkBehaviour
	{
		[SerializeField] private PlayerAvatarPrefabs m_AvatarPrefabs;

		// TODO: avater index var...
		private NetworkVariable<Byte> m_AvatarIndex = new();
		private PlayerAvatar m_Avatar;
		public PlayerAvatar Avatar { get => m_Avatar; private set => m_Avatar = value; }
		internal PlayerAvatarPrefabs AvatarPrefabs => m_AvatarPrefabs;

		private void Awake() => m_Avatar = new PlayerAvatar(this);


	}
	public sealed class PlayerAvatar
	{
		private readonly LocalPlayer m_Player;

		private GameObject m_Active;

		private int m_ActiveIndex;
		public Int32 ActiveIndex => m_ActiveIndex;

		public PlayerAvatar(LocalPlayer player) => m_Player = player;

		public void Select(Int32 avatarIndex)
		{
			if (m_Active != null)
				Object.Destroy(m_Active);

			var prefab = m_Player.AvatarPrefabs.GetPrefab(avatarIndex);
			m_Active = Object.Instantiate<GameObject>(prefab, m_Player.transform);
			m_ActiveIndex = avatarIndex;
		}
	}
}
