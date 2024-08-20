// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Player
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerServerRpc))]
	public sealed class PlayerClientRpc : NetworkBehaviour
	{
		private readonly TaskCompletionSource<LocalPlayer>[] m_SpawnTcs =
			new TaskCompletionSource<LocalPlayer>[LocalPlayers.MaxLocalPlayers];

		private PlayerServerRpc m_ServerRpc;

		private void Awake() => m_ServerRpc = GetComponent<PlayerServerRpc>();

		public Task<LocalPlayer> Spawn(Int32 localPlayerIndex, Int32 avatarIndex)
		{
			if (m_SpawnTcs[localPlayerIndex] != null)
				throw new Exception($"spawn already in progress, player index: {localPlayerIndex}");

			m_SpawnTcs[localPlayerIndex] = new TaskCompletionSource<LocalPlayer>();
			m_ServerRpc.SpawnPlayerServerRpc(localPlayerIndex, avatarIndex, OwnerClientId);
			return m_SpawnTcs[localPlayerIndex].Task;
		}

		[Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
		internal void DidSpawnPlayerClientRpc(NetworkObjectReference playerRef, Int32 localPlayerIndex)
		{
			if (IsOwner)
			{
				// this should not fail thus no error check
				playerRef.TryGet(out var playerObj);

				var localPlayer = playerObj.GetComponent<LocalPlayer>();

				// end awaitable task, and discard
				m_SpawnTcs[localPlayerIndex].SetResult(localPlayer);
				m_SpawnTcs[localPlayerIndex] = null;
			}
		}

		public void SetAvatar(NetworkObjectReference playerRef, int avatarIndex)
		{
			m_ServerRpc.SetAvatarServerRpc(playerRef, avatarIndex);
		}

		[Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
		public void SetAvatarClientRpc(NetworkObjectReference playerRef, int avatarIndex)
		{
			playerRef.TryGet(out var playerObj);
			var localPlayer = playerObj.GetComponent<LocalPlayer>();
			localPlayer.Avatar.Select(avatarIndex);
		}
	}
}
