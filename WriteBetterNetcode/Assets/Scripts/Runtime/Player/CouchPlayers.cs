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
				var userState = Components.InputUserState;
				userState.OnDevicePaired += OnInputDevicePaired;
				userState.OnDeviceUnpaired += OnInputDeviceUnpaired;
				userState.PairingEnabled = true;

				// always spawn the host player
				await SpawnPlayer(0, 0);

				//Test_SpawnPlayers();
				//StartCoroutine(Test_ShuffleAvatar());
			}
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			if (IsOwner)
			{
				StopAllCoroutines();

				var userState = Components.InputUserState;
				userState.PairingEnabled = false;
				userState.OnDevicePaired -= OnInputDevicePaired;
				userState.OnDeviceUnpaired -= OnInputDeviceUnpaired;
			}
		}

		private async void OnInputDevicePaired(InputUser user, InputDevice device) => await SpawnPlayer(user.index, user.index);

		private void OnInputDeviceUnpaired(InputUser user, InputDevice device)
		{
			var playerIndex = user.index;
			var playerObj = m_Players[playerIndex].GetComponent<NetworkObject>();
			m_Players[playerIndex] = null;

			m_ClientSide.Despawn(playerObj);
		}

		private async Task SpawnPlayer(Int32 playerIndex, Int32 avatarIndex)
		{
			var posX = -3f + playerIndex * 2f;
			var posY = OwnerClientId * 2f;
			var position = new Vector3(posX, posY, 0);

			m_Players[playerIndex] = await m_ClientSide.Spawn(position, playerIndex, avatarIndex);
			SetPlayerDebugName(playerIndex);
		}

		public void AddRemotePlayer(Player player, Int32 playerIndex)
		{
			if (m_Players[playerIndex] != null)
				throw new Exception($"player {playerIndex} already exists");

			m_Players[playerIndex] = player;
			SetPlayerDebugName(playerIndex, " (Remote)");
		}

		/*
			    private static Int32 Test_ShuffleAvatarIndex(Player playerAvatar)
		{
			var curAvatarIndex = playerAvatar.AvatarIndex;
			var newAvatarIndex = 0;

			// poor dev's shuffle
			do
			{
				newAvatarIndex = Random.Range(0, 5);
			} while (curAvatarIndex == newAvatarIndex);

			return newAvatarIndex;
		}

		private async void Test_SpawnPlayers()
		{
			Random.InitState(DateTime.Now.Millisecond);

			var posY = OwnerClientId * 2f;
			m_Players[0] = await m_ClientSide.Spawn(0, 0);
			SetPlayerDebugName(0);
			m_Players[0].transform.position = new Vector3(-3, posY, 0);

			m_Players[1] = await m_ClientSide.Spawn(1, 1);
			SetPlayerDebugName(1);
			m_Players[1].transform.position = new Vector3(-1, posY, 0);

			m_Players[2] = await m_ClientSide.Spawn(2, 2);
			SetPlayerDebugName(2);
			m_Players[2].transform.position = new Vector3(1, posY, 0);

			m_Players[3] = await m_ClientSide.Spawn(3, 3);
			SetPlayerDebugName(3);
			m_Players[3].transform.position = new Vector3(3, posY, 0);
		}

		private IEnumerator Test_ShuffleAvatar()
		{
			do
			{
				for (var i = 0; i < Constants.MaxCouchPlayers; i++)
				{
					yield return new WaitForSecondsRealtime(1.132473199f);

					if (m_Players[i] != null)
					{
						var avatarIndex = Test_ShuffleAvatarIndex(m_Players[i]);
						m_Players[i].AvatarIndex = (Byte)avatarIndex;
					}
				}
			} while (true);
		}
		*/
	}
}
