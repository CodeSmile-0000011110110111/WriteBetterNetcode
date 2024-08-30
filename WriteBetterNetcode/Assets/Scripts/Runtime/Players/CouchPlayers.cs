// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Settings;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace CodeSmile.Players
{
	// TODO: detect and pair devices at runtime
	// FIXME: on "join" prevent "leave" nullref caused by player not yet spawned

	/// <summary>
	///     Represents the group of players (1-4) playing on a single client.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CouchPlayersClient),
		typeof(CouchPlayersServer))]
	public sealed class CouchPlayers : NetworkBehaviour
	{
		public static event Action<CouchPlayers> OnCouchSessionStarted;
		public static event Action OnCouchSessionStopped;

		public event Action<Int32> OnCouchPlayerJoin;
		public event Action<Int32> OnCouchPlayerLeave;

		private readonly Player[] m_Players = new Player[Constants.MaxCouchPlayers];
		private readonly Status[] m_PlayerStatus = new Status[Constants.MaxCouchPlayers];

		private CouchPlayersClient m_ClientSide;

		public Player this[Int32 index] => index >= 0 && index < Constants.MaxCouchPlayers ? m_Players[index] : null;

		private void Awake() => m_ClientSide = GetComponent<CouchPlayersClient>();

		private void SetPlayerDebugName(Int32 playerIndex, String suffix = "") => m_Players[playerIndex].name =
			m_Players[playerIndex].name.Replace("(Clone)", $" #{playerIndex}{suffix}");

		public override async void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner)
			{
				var inputUsers = Components.InputUsers;
				inputUsers.OnDevicePaired += OnInputDevicePaired;
				inputUsers.OnDeviceUnpaired += OnInputDeviceUnpaired;
				inputUsers.AllPairingEnabled = true;
				inputUsers.AllUiEnabled = false;
				inputUsers.AllPlayerInteractionEnabled = true;
				inputUsers.AllPlayerKinematicsEnabled = true;
				inputUsers.AllPlayerUiEnabled = true;

				OnCouchSessionStarted?.Invoke(this);

				await SpawnPlayer(0, 0); // spawn host player
			}
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			if (IsOwner)
			{
				var inputUsers = Components.InputUsers;
				inputUsers.AllPairingEnabled = false;
				inputUsers.AllUiEnabled = true;
				inputUsers.AllPlayerInteractionEnabled = false;
				inputUsers.AllPlayerKinematicsEnabled = false;
				inputUsers.AllPlayerUiEnabled = false;
				inputUsers.OnDevicePaired -= OnInputDevicePaired;
				inputUsers.OnDeviceUnpaired -= OnInputDeviceUnpaired;
				inputUsers.UnpairAll();

				DespawnAllPlayers();

				OnCouchSessionStopped?.Invoke();
			}
		}

		private void DespawnAllPlayers()
		{
			for (var playerIndex = m_Players.Length - 1; playerIndex >= 0; playerIndex--)
				DespawnPlayer(playerIndex);
		}

		private async void OnInputDevicePaired(InputUser user, InputDevice device) => await TrySpawnPlayer(user);
		private void OnInputDeviceUnpaired(InputUser user, InputDevice device) => DespawnPlayer(user.index);

		private async Task TrySpawnPlayer(InputUser user)
		{
			if (m_PlayerStatus[user.index] != Status.Available)
				return;

			await SpawnPlayer(user.index, user.index);
		}

		private async Task SpawnPlayer(Int32 playerIndex, Int32 avatarIndex)
		{
			var posX = -3f + playerIndex * 2f;
			var posY = OwnerClientId * 2f;
			var position = new Vector3(posX, posY, 0);

			m_PlayerStatus[playerIndex] = Status.Spawning;

			var player = await m_ClientSide.Spawn(position, playerIndex, avatarIndex);
			m_Players[playerIndex] = player;
			m_PlayerStatus[playerIndex] = Status.Spawned;
			SetPlayerDebugName(playerIndex);

			player.OnPlayerSpawn(playerIndex);
			OnCouchPlayerJoin?.Invoke(playerIndex);
		}

		private void DespawnPlayer(Int32 playerIndex)
		{
			var player = m_Players[playerIndex];
			if (player != null)
			{
				OnCouchPlayerLeave?.Invoke(playerIndex);
				player.OnPlayerDespawn(playerIndex);
				ResetPlayerFields(playerIndex);

				var playerObj = player.GetComponent<NetworkObject>();
				m_ClientSide.Despawn(playerObj);
			}
		}

		private void ResetPlayerFields(Int32 playerIndex)
		{
			m_Players[playerIndex] = null;
			m_PlayerStatus[playerIndex] = Status.Available;
		}

		public void AddRemotePlayer(Player player, Int32 playerIndex)
		{
			if (m_Players[playerIndex] != null)
				throw new Exception($"player {playerIndex} already exists");

			m_Players[playerIndex] = player;
			SetPlayerDebugName(playerIndex, " (Remote)");
		}

		private enum Status
		{
			Available,
			Spawning,
			Spawned,
		}
	}
}
