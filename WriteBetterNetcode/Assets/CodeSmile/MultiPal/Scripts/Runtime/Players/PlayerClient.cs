// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Animation;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players
{
	[DisallowMultipleComponent]
	internal class PlayerClient : NetworkBehaviour
	{
		private PlayerServer m_ServerSide;

		private void Awake() => m_ServerSide = GetComponent<PlayerServer>();

		public void SyncAnimatorParameters(AvatarAnimatorParameters avatarAnimatorParameters) =>
			SyncAnimatorParametersToNonOwnersRpc(avatarAnimatorParameters);

		[Rpc(SendTo.NotOwner, DeferLocal = true)]
		private void SyncAnimatorParametersToNonOwnersRpc(AvatarAnimatorParameters avatarAnimatorParameters)
		{
			var avatar = GetComponent<PlayerAvatar>();
			avatar.ReceiveAnimatorParameters(avatarAnimatorParameters);
		}
	}
}
