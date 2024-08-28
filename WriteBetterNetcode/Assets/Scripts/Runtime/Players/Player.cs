// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerAvatar), typeof(PlayerInputActions))]
	[RequireComponent(typeof(PlayerVars), typeof(PlayerServer), typeof(PlayerClient))]
	public sealed class Player : NetworkBehaviour
	{
		public event Action<Player> OnRequestPause;

		private PlayerAvatar m_Avatar;
		private PlayerClient m_ClientSide;
		private PlayerVars m_Vars;

		public Byte AvatarIndex { get => m_Vars.AvatarIndex; set => m_Vars.AvatarIndex = value; }

		public int PlayerIndex { get; private set; } = -1;

		private void Awake()
		{
			m_Avatar = GetComponent<PlayerAvatar>();
			m_ClientSide = GetComponent<PlayerClient>();
			m_Vars = GetComponent<PlayerVars>();
		}

		internal void OnAvatarIndexChanged(Byte _, Byte avatarIndex) => m_Avatar.SetAvatar(avatarIndex);

		internal void OnCouchPlayerSpawn(int playerIndex)
		{
			PlayerIndex = playerIndex;

			var input = GetComponent<PlayerInputActions>();
			input.RegisterCallback(PlayerIndex);
			input.RequestPause += RequestPause;
		}

		internal void OnCouchPlayerDespawn()
		{
			var input = GetComponent<PlayerInputActions>();
			input.UnregisterCallback(PlayerIndex);
		}

		private void RequestPause()
		{
			OnRequestPause?.Invoke(this);
		}
	}
}
