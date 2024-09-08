// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Input;
using CodeSmile.MultiPal.Settings;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace CodeSmile.MultiPal.Players.Couch
{
	// TODO: detect and pair devices at runtime
	// FIXME: on "join" prevent "leave" nullref caused by player not yet spawned

	/// <summary>
	///     Represents the group of players (1-4) playing on a single client.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CouchPlayersClient), typeof(CouchPlayersServer))]
	[RequireComponent(typeof(CouchPlayersVars))]
	public sealed class CouchPlayers : NetworkBehaviour
	{
		public static event Action<CouchPlayers> OnLocalCouchPlayersSpawn;
		public static event Action<CouchPlayers> OnLocalCouchPlayersDespawn;
		public event Action<CouchPlayers, Int32> OnCouchPlayerJoining;
		public event Action<CouchPlayers, Int32> OnCouchPlayerJoined;
		public event Action<CouchPlayers, Int32> OnCouchPlayerLeaving;
		public event Action<CouchPlayers, Int32> OnCouchPlayerLeft;

		// serialized only for debugging
		[SerializeField] private Player[] m_Players = new Player[Constants.MaxCouchPlayers];
		[SerializeField] private Status[] m_PlayerStatus = new Status[Constants.MaxCouchPlayers];

		private CouchPlayersClient m_ClientSide;
		private CouchPlayersVars m_Vars;

		public Player this[Int32 index] => index >= 0 && index < Constants.MaxCouchPlayers ? m_Players[index] : null;

		public Int32 PlayerCount { get; set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticFields()
		{
			OnLocalCouchPlayersSpawn = null;
			OnLocalCouchPlayersDespawn = null;
		}

		private void Awake()
		{
			m_ClientSide = GetComponent<CouchPlayersClient>();
			m_Vars = GetComponent<CouchPlayersVars>();

			m_Players = new Player[Constants.MaxCouchPlayers];
			m_PlayerStatus = new Status[Constants.MaxCouchPlayers];
		}

		private void SetCouchPlayersDebugName() => gameObject.name =
			gameObject.name.Replace("(Clone)", $" (ID:{NetworkObjectId}) {(IsOwner ? "<== LOCAL" : "")}");

		private void SetPlayerDebugName(Int32 playerIndex, String suffix = "") => m_Players[playerIndex].name =
			m_Players[playerIndex]
				.name.Replace("(Clone)", $" (CP:{NetworkObjectId}, " +
				                         $"ID:{m_Players[playerIndex].NetworkObjectId}) " +
				                         $"P{playerIndex}{suffix}");

		public override async void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			SetCouchPlayersDebugName();

			if (IsOwner)
			{
				ComponentsRegistry.Set(this);
				OnLocalCouchPlayersSpawn?.Invoke(this);

				var inputUsers = ComponentsRegistry.Get<InputUsers>();
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
				DespawnAllPlayers();

				OnLocalCouchPlayersDespawn?.Invoke(this);
				ComponentsRegistry.Set<CouchPlayers>(null);

				var inputUsers = ComponentsRegistry.Get<InputUsers>();
				inputUsers.AllPairingEnabled = false;
				inputUsers.AllUiEnabled = true;
				inputUsers.AllPlayerInteractionEnabled = false;
				inputUsers.AllPlayerKinematicsEnabled = false;
				inputUsers.AllPlayerUiEnabled = false;
				inputUsers.OnUserDevicePaired -= OnUserInputDevicePaired;
				inputUsers.OnUserDeviceUnpaired -= OnUserInputDeviceUnpaired;
				inputUsers.UnpairAll();
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
			var startPos = Vector3.zero; // TODO: get start pos

			m_PlayerStatus[playerIndex] = Status.Spawning;
			OnCouchPlayerJoining?.Invoke(this, playerIndex);

			var player = await m_ClientSide.Spawn(startPos, playerIndex, avatarIndex);
			m_Players[playerIndex] = player;
			m_PlayerStatus[playerIndex] = Status.Spawned;
			PlayerCount++;

			SetPlayerDebugName(playerIndex, " <== LOCAL");

			player.OnPlayerSpawn(playerIndex, IsOwner);

			OnCouchPlayerJoined?.Invoke(this, playerIndex);
		}

		private void DespawnPlayer(Int32 playerIndex)
		{
			var player = m_Players[playerIndex];
			if (player != null)
			{
				OnCouchPlayerLeaving?.Invoke(this, playerIndex);

				player.OnPlayerDespawn(playerIndex, IsOwner);
				m_PlayerStatus[playerIndex] = Status.Available;
				PlayerCount--;

				var playerObj = player.GetComponent<NetworkObject>();
				m_ClientSide.Despawn(playerIndex, playerObj);

				OnCouchPlayerLeft?.Invoke(this, playerIndex);
				m_Players[playerIndex] = null;
			}
		}

		internal void RemotePlayerJoined(Int32 playerIndex, Player player)
		{
			if (m_Players[playerIndex] != null)
				throw new Exception($"remote player {playerIndex} already exists");

			m_Players[playerIndex] = player;
			m_PlayerStatus[playerIndex] = Status.Spawned;
			SetPlayerDebugName(playerIndex);

			player.OnPlayerSpawn(playerIndex, false);
		}

		internal void RemotePlayerLeft(Int32 playerIndex)
		{
			// Note: OnPlayerDespawn is called in Player via OnNetworkDespawn since by this point remote Player is already destroyed
			m_Players[playerIndex] = null;
			m_PlayerStatus[playerIndex] = Status.Available;
		}

		private enum Status
		{
			Available,
			Spawning,
			Spawned,
		}
	}
}
