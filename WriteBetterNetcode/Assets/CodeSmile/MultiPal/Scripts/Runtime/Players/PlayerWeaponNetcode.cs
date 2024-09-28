// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Extensions.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.MultiPal.Players.Couch
{
	[DisallowMultipleComponent]
	public sealed class PlayerWeaponNetcode : NetworkBehaviour
	{
		public void StartAttacking()
		{
			if (NetworkManagerExt.IsOffline)
				StartAttackingServerSide();
			else
				StartAttackingServerRpc();
		}

		public void StopAttacking()
		{
			if (NetworkManagerExt.IsOffline)
				StopAttackingServerSide();
			else
				StopAttackingServerRpc();
		}

		private void StartAttackingServerSide() {}
		private void StopAttackingServerSide() {}

		[Rpc(SendTo.Everyone, DeferLocal = true, RequireOwnership = true)]
		private void StartAttackingServerRpc() => StartAttackingServerSide();

		[Rpc(SendTo.Everyone, DeferLocal = true, RequireOwnership = true)]
		private void StopAttackingServerRpc() => StopAttackingServerSide();
	}
}
