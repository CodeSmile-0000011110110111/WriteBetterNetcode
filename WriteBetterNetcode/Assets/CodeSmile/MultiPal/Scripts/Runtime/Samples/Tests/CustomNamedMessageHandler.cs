// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Samples.Tests
{
	public class CustomNamedMessageHandler : MonoBehaviour
	{
		private readonly String kMessageName = nameof(CustomNamedMessageHandler);

		private NetworkManager NetworkManager => NetworkManager.Singleton;
		private Boolean IsServer => NetworkManager.Singleton.IsServer;

		//public override void OnNetworkSpawn()
		private void OnEnable()
		{
			if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
			{
				NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(kMessageName, ReceiveMessageInternal);

				if (IsServer)
					NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
				else
				{
					var guid = Guid.NewGuid();
					Debug.Log($"<color=yellow>Client sending GUID ({guid}) to server.</color>");
					SendMessage(guid);
				}
			}
		}

		//public override void OnNetworkDespawn()
		private void OnDisable()
		{
			if (NetworkManager != null)
			{
				NetworkManager.CustomMessagingManager?.UnregisterNamedMessageHandler(kMessageName);
				NetworkManager.OnClientDisconnectCallback -= OnClientConnectedCallback;
			}
		}

		private void OnClientConnectedCallback(UInt64 obj) => SendMessage(Guid.NewGuid());

		public void SendMessage(Guid inGameIdentifier)
		{
			var content = new ForceNetworkSerializeByMemcpy<Guid>(inGameIdentifier);
			using var payload = new FastBufferWriter(1100, Allocator.Temp);
			payload.WriteValueSafe(content);

			SendMessageInternal(payload);
		}

		private void SendMessageInternal(FastBufferWriter payload)
		{
			var msgManager = NetworkManager.CustomMessagingManager;

			if (IsServer)
				msgManager.SendNamedMessageToAll(kMessageName, payload);
			else
				msgManager.SendNamedMessage(kMessageName, NetworkManager.ServerClientId, payload);
		}

		private void ReceiveMessageInternal(UInt64 senderId, FastBufferReader payload)
		{
			var content = new ForceNetworkSerializeByMemcpy<Guid>(new Guid());
			payload.ReadValueSafe(out content);

			OnMessageReceived(senderId, content);
		}

		private void OnMessageReceived(UInt64 senderId, ForceNetworkSerializeByMemcpy<Guid> content)
		{
			if (IsServer)
				Debug.Log($"<color=red>Sever received GUID ({content.Value}) from client ({senderId})</color>");
			else
				Debug.Log($"<color=red>Client received GUID ({content.Value}) from the server.</color>");
		}
	}
}
