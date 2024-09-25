// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.Extensions.Netcode;
using CodeSmile.MultiPal.Design;
using CodeSmile.MultiPal.Input;
using CodeSmile.MultiPal.Settings;
using System;
using System.Collections;
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
	[RequireComponent(typeof(CouchPlayersVars), typeof(CouchPlayersClient), typeof(CouchPlayersServer))]
	public sealed class CouchPlayers : NetworkBehaviour, IEnumerable
	{
		public static event Action<CouchPlayers> OnLocalCouchPlayersSpawn;
		public static event Action<CouchPlayers> OnLocalCouchPlayersDespawn;
		public event Action<CouchPlayers, Int32> OnCouchPlayerJoining;
		public event Action<CouchPlayers, Int32> OnCouchPlayerJoined;
		public event Action<CouchPlayers, Int32> OnCouchPlayerLeaving;
		public event Action<CouchPlayers, Int32> OnCouchPlayerLeft;

		private Player[] m_Players = new Player[Constants.MaxCouchPlayers];
		private Status[] m_PlayerStatus = new Status[Constants.MaxCouchPlayers];

		private CouchPlayersClient m_ClientSide;
		private CouchPlayersVars m_Vars;

		public Player this[Int32 index] => index >= 0 && index < Constants.MaxCouchPlayers ? m_Players[index] : null;
		public Int32 PlayerCount { get; set; }
		private Boolean IsOffline => NetworkManagerExt.IsOffline;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticFields()
		{
			OnLocalCouchPlayersSpawn = null;
			OnLocalCouchPlayersDespawn = null;
		}

		public IEnumerator GetEnumerator() => m_Players.GetEnumerator();

		private void Awake()
		{
			m_ClientSide = GetComponent<CouchPlayersClient>();
			m_Vars = GetComponent<CouchPlayersVars>();

			m_Players = new Player[Constants.MaxCouchPlayers];
			m_PlayerStatus = new Status[Constants.MaxCouchPlayers];
		}

		private void OnEnable()
		{
			if (IsOffline)
				StartCoroutine(CallOnNetworkSpawnAfterDelay());
		}

		private void OnDisable()
		{
			if (IsOffline)
				OnNetworkDespawn();
		}

		public Boolean IsPlaying(Int32 playerIndex) => m_PlayerStatus[playerIndex] == Status.Spawned;

		private IEnumerator CallOnNetworkSpawnAfterDelay()
		{
			// WebGL builds require a little delay to ensure correct execution order of events,
			// eg we would otherwise call SpawnPlayer before that component's Awake ran for whatever reason
			yield return new WaitForEndOfFrame();

			OnNetworkSpawn();
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

			if (IsOwner || IsOffline)
			{
				ComponentsRegistry.Set(this);

				var inputUsers = ComponentsRegistry.Get<InputUsers>();
				inputUsers.OnUserDevicePaired += OnUserInputDevicePaired;
				inputUsers.OnUserDeviceUnpaired += OnUserInputDeviceUnpaired;
				inputUsers.AllPairingEnabled = true;
				inputUsers.AllUiEnabled = false;
				inputUsers.AllPlayerInteractionEnabled = true;
				inputUsers.AllPlayerKinematicsEnabled = true;
				inputUsers.AllPlayerUiEnabled = true;

				OnLocalCouchPlayersSpawn?.Invoke(this);

				await StartSpawnPlayers();
			}
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			if (IsOwner || IsOffline)
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

				SpawnLocations.OnSpawnLocationsChanged -= OnSpawnLocationsChanged;
			}
		}

		private void DespawnAllPlayers()
		{
			for (var playerIndex = m_Players.Length - 1; playerIndex >= 0; playerIndex--)
				DespawnPlayer(playerIndex);
		}

		private async void OnUserInputDevicePaired(InputUser user, InputDevice device) => await TrySpawnPlayer(user);
		private void OnUserInputDeviceUnpaired(InputUser user, InputDevice device) => DespawnPlayer(user.index);

		private async void OnSpawnLocationsChanged(SpawnLocations spawnLocations)
		{
			if (SpawnLocations.Count > 0)
			{
				SpawnLocations.OnSpawnLocationsChanged -= OnSpawnLocationsChanged;
				await StartSpawnPlayers();
			}
		}

		public async Task StartSpawnPlayers()
		{
			if (IsOwner || IsOffline)
			{
				if (SpawnLocations.Count == 0)
					SpawnLocations.OnSpawnLocationsChanged += OnSpawnLocationsChanged;
				else
				{
					var inputUsers = ComponentsRegistry.Get<InputUsers>();
					foreach (var pairedUser in inputUsers.PairedUsers)
					{
						// FIXME: change to spawn multiple at once
						var playerIndex = pairedUser.index;
						var avatarIndex = pairedUser.index;
						await SpawnPlayer(playerIndex, avatarIndex);
					}
				}
			}
		}

		private async Task TrySpawnPlayer(InputUser user)
		{
			var playerIndex = user.index;
			if (m_PlayerStatus[playerIndex] != Status.Available)
			{
				Debug.LogWarning($"can't spawn player {playerIndex} with status {m_PlayerStatus[playerIndex]} for user {user}");
				return;
			}

			var avatarIndex = playerIndex;
			await SpawnPlayer(playerIndex, avatarIndex);
		}

		private async Task SpawnPlayer(Int32 playerIndex, Int32 avatarIndex)
		{
			m_PlayerStatus[playerIndex] = Status.Spawning;
			OnCouchPlayerJoining?.Invoke(this, playerIndex);

			var player = await m_ClientSide.SpawnPlayer(playerIndex, avatarIndex);

			m_Players[playerIndex] = player;
			m_PlayerStatus[playerIndex] = Status.Spawned;
			PlayerCount++;

			SetPlayerDebugName(playerIndex, " <== LOCAL");

			player.OnPlayerSpawn(playerIndex, IsOwner || IsOffline);
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
				m_ClientSide.DespawnPlayer(playerIndex, playerObj);

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
