// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Game
{
	public sealed class LocalPlayer : NetworkBehaviour
	{
		[SerializeField] private PlayerAvatarPrefabs m_AvatarPrefabs;

		// TODO: avater index var...
		private NetworkVariable<Byte> m_AvatarIndex = new();
		public LocalPlayerAvatar Avatar { get; private set; }
		internal PlayerAvatarPrefabs AvatarPrefabs => m_AvatarPrefabs;

		private void Awake() => Avatar = new LocalPlayerAvatar(this);
	}
}
