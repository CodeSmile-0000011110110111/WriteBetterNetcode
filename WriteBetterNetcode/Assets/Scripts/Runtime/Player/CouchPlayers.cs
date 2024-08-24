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

namespace CodeSmile.Player
{
	/// <summary>
	///     Represents the group of players (1-4) playing on a single client.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CouchPlayersClient),
		typeof(CouchPlayersServer))]
	public sealed class CouchPlayers : NetworkBehaviour
	{
		private readonly Player[] m_Players = new Player[Constants.MaxCouchPlayers];

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
				inputUsers.PairingEnabled = Application.platform != RuntimePlatform.WebGLPlayer;

				// always spawn the host player
				await SpawnPlayer(0, 0);
			}
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			if (IsOwner)
			{
				StopAllCoroutines();

				var inputUsers = Components.InputUsers;
				inputUsers.PairingEnabled = false;
				inputUsers.OnDevicePaired -= OnInputDevicePaired;
				inputUsers.OnDeviceUnpaired -= OnInputDeviceUnpaired;
			}
		}

		private async void OnInputDevicePaired(InputUser user, InputDevice device) => await SpawnPlayer(user.index, user.index);
		private void OnInputDeviceUnpaired(InputUser user, InputDevice device) => DespawnPlayer(user.index);

		private async Task SpawnPlayer(Int32 playerIndex, Int32 avatarIndex)
		{
			var posX = -3f + playerIndex * 2f;
			var posY = OwnerClientId * 2f;
			var position = new Vector3(posX, posY, 0);

			m_Players[playerIndex] = await m_ClientSide.Spawn(position, playerIndex, avatarIndex);
			SetPlayerDebugName(playerIndex);

			Components.InputUsers.SetPlayerActionsEnabled(playerIndex, true);
		}

		private void DespawnPlayer(Int32 playerIndex)
		{
			var playerObj = m_Players[playerIndex].GetComponent<NetworkObject>();
			m_Players[playerIndex] = null;

			Components.InputUsers.SetPlayerActionsEnabled(playerIndex, false);

			m_ClientSide.Despawn(playerObj);
		}

		public void AddRemotePlayer(Player player, Int32 playerIndex)
		{
			if (m_Players[playerIndex] != null)
				throw new Exception($"player {playerIndex} already exists");

			m_Players[playerIndex] = player;
			SetPlayerDebugName(playerIndex, " (Remote)");
		}
	}
}
