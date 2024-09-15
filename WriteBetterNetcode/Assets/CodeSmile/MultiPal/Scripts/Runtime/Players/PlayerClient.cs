// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Animation;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players
{
	[DisallowMultipleComponent]
	public class PlayerClient : NetworkBehaviour
	{
		private Player m_Player;
		private PlayerServer m_ServerSide;
		private IAnimatorController m_AnimatorController;
		private AvatarAnimatorParameters m_AnimatorParameters;
		public IAnimatorController AnimatorController
		{
			get => m_AnimatorController;
			set => m_AnimatorController = value;
		}
		public AvatarAnimatorParameters AnimatorParameters
		{
			get => m_AnimatorParameters;
			set => m_AnimatorParameters = value;
		}

		private void Awake()
		{
			m_Player = GetComponent<Player>();
			m_ServerSide = GetComponent<PlayerServer>();
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner)
				NetworkManager.NetworkTickSystem.Tick += OnNetworkTick;
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			if (IsOwner)
				NetworkManager.NetworkTickSystem.Tick -= OnNetworkTick;
		}

		private void OnNetworkTick()
		{
			if (m_AnimatorParameters != null)
				SyncAnimatorParametersToNonOwnersRpc(m_AnimatorParameters.Parameters);
		}

		[Rpc(SendTo.NotOwner, DeferLocal = true)]
		private void SyncAnimatorParametersToNonOwnersRpc(Byte[] animatorParameters)
		{
			if (m_AnimatorController != null)
				m_AnimatorController.RemoteAnimatorParametersReceived(animatorParameters);
		}

		[Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
		public void KillPlayerClientRpc()
		{
			m_Player.Die();
		}

		[Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
		public void RespawnPlayerClientRpc()
		{
			m_Player.Respawn();
		}
	}
}
