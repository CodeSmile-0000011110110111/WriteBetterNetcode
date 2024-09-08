// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Animation;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players
{
	[DisallowMultipleComponent]
	public class PlayerClient : NetworkBehaviour
	{
		private PlayerServer m_ServerSide;
		private IAnimatorController m_AnimatorController;
		public IAnimatorController AnimatorController
		{
			get => m_AnimatorController;
			set => m_AnimatorController = value;
		}

		private void Awake() => m_ServerSide = GetComponent<PlayerServer>();

		public void SendAnimatorParametersToNonOwners(byte[] animatorParameters) =>
			SyncAnimatorParametersToNonOwnersRpc(animatorParameters);

		[Rpc(SendTo.NotOwner, DeferLocal = true)]
		private void SyncAnimatorParametersToNonOwnersRpc(byte[] animatorParameters)
		{
			if (m_AnimatorController != null)
				m_AnimatorController.RemoteAnimatorParametersReceived(animatorParameters);
		}
	}
}
