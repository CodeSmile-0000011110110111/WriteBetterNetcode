// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Settings;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace CodeSmile.MultiPal.Player
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
		public event Action<CouchPlayers, Int32> OnCouchPlayerJoining;
		public event Action<CouchPlayers, Int32> OnCouchPlayerJoined;
		public event Action<CouchPlayers, Int32> OnCouchPlayerLeaving;
		public event Action<CouchPlayers, Int32> OnCouchPlayerLeft;

		private readonly Player[] m_Players = new Player[Constants.MaxCouchPlayers];
		private readonly Status[] m_PlayerStatus = new Status[Constants.MaxCouchPlayers];

		private CouchPlayersClient m_ClientSide;

		public Player this[Int32 index] => index >= 0 && index < Constants.MaxCouchPlayers ? m_Players[index] : null;

		public int PlayerCount { get; set; }

		private void Awake() => m_ClientSide = GetComponent<CouchPlayersClient>();

		private void SetPlayerDebugName(Int32 playerIndex, String suffix = "") => m_Players[playerIndex].name =
			m_Players[playerIndex].name.Replace("(Clone)", $" #{playerIndex}{suffix}");

		public override async void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner)
			{
				Global.Components.LocalCouchPlayers = this;

				var inputUsers = Global.Components.InputUsers;
				inputUsers.OnUserDevicePaired += OnUserInputDevicePaired;
				inputUsers.OnUserDeviceUnpaired += OnUserInputDeviceUnpaired;
				inputUsers.AllPairingEnabled = true;
				inputUsers.AllUiEnabled = false;
				inputUsers.AllPlayerInteractionEnabled = true;
				inputUsers.AllPlayerKinematicsEnabled = true;
				inputUsers.AllPlayerUiEnabled = true;

				await SpawnPlayer(0, 0); // spawn host player
			}
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			if (IsOwner)
			{
				Global.Components.LocalCouchPlayers = null;

				var inputUsers = Global.Components.InputUsers;
				inputUsers.AllPairingEnabled = false;
				inputUsers.AllUiEnabled = true;
				inputUsers.AllPlayerInteractionEnabled = false;
				inputUsers.AllPlayerKinematicsEnabled = false;
				inputUsers.AllPlayerUiEnabled = false;
				inputUsers.OnUserDevicePaired -= OnUserInputDevicePaired;
				inputUsers.OnUserDeviceUnpaired -= OnUserInputDeviceUnpaired;
				inputUsers.UnpairAll();

				DespawnAllPlayers();
			}
		}

		private void DespawnAllPlayers()
		{
			for (var playerIndex = m_Players.Length - 1; playerIndex >= 0; playerIndex--)
				DespawnPlayer(playerIndex);
		}

		private async void OnUserInputDevicePaired(InputUser user, InputDevice device) => await TrySpawnPlayer(user);
		private void OnUserInputDeviceUnpaired(InputUser user, InputDevice device) => DespawnPlayer(user.index);

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
			OnCouchPlayerJoining?.Invoke(this, playerIndex);

			var player = await m_ClientSide.Spawn(position, playerIndex, avatarIndex);
			m_Players[playerIndex] = player;
			m_PlayerStatus[playerIndex] = Status.Spawned;
			SetPlayerDebugName(playerIndex);

			player.OnPlayerSpawn(playerIndex);
			PlayerCount++;

			OnCouchPlayerJoined?.Invoke(this, playerIndex);
		}

		private void DespawnPlayer(Int32 playerIndex)
		{
			var player = m_Players[playerIndex];
			if (player != null)
			{
				OnCouchPlayerLeaving?.Invoke(this, playerIndex);

				player.OnPlayerDespawn(playerIndex);
				m_Players[playerIndex] = null;
				m_PlayerStatus[playerIndex] = Status.Available;
				PlayerCount--;

				var playerObj = player.GetComponent<NetworkObject>();
				m_ClientSide.Despawn(playerObj);

				OnCouchPlayerLeft?.Invoke(this, playerIndex);
			}
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
