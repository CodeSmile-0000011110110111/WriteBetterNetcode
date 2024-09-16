// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Samples.Tests
{
	public class CustomNamedMessageHandler : NetworkBehaviour
	{
		[Tooltip("The name identifier used for this custom message handler.")]
		public String kMessageName = "MyCustomNamedMessage";

		/// <summary>
		///     For most cases, you want to register once your NetworkBehaviour's
		///     NetworkObject (typically in-scene placed) is spawned.
		/// </summary>
		public override void OnNetworkSpawn()
		{
			// Both the server-host and client(s) register the custom named message.
			NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(kMessageName, ReceiveMessage);

			if (IsServer)
			{
				// Server broadcasts to all clients when a new client connects (just for example purposes)
				NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
			}
			else
			{
				// Clients send a unique Guid to the server
				var guid = Guid.NewGuid();
				Debug.Log($"<color=yellow>Client sending GUID ({guid}) to server.</color>");
				SendMessage(guid);
			}
		}

		private void OnClientConnectedCallback(UInt64 obj) => SendMessage(Guid.NewGuid());

		public override void OnNetworkDespawn()
		{
			// De-register when the associated NetworkObject is despawned.
			NetworkManager.CustomMessagingManager?.UnregisterNamedMessageHandler(kMessageName);
			// Whether server or not, unregister this.
			NetworkManager.OnClientDisconnectCallback -= OnClientConnectedCallback;
		}

		/// <summary>
		///     Invoked when a custom message of type <see cref="kMessageName" />
		/// </summary>
		private void ReceiveMessage(UInt64 senderId, FastBufferReader messagePayload)
		{
			var receivedMessageContent = new ForceNetworkSerializeByMemcpy<Guid>(new Guid());
			messagePayload.ReadValueSafe(out receivedMessageContent);
			if (IsServer)
				Debug.Log($"<color=red>Sever received GUID ({receivedMessageContent.Value}) from client ({senderId})</color>");
			else
				Debug.Log($"<color=red>Client received GUID ({receivedMessageContent.Value}) from the server.</color>");
		}

		/// <summary>
		///     Invoke this with a Guid by a client or server-host to send a
		///     custom named message.
		/// </summary>
		public void SendMessage(Guid inGameIdentifier)
		{
			var messageContent = new ForceNetworkSerializeByMemcpy<Guid>(inGameIdentifier);
			var writer = new FastBufferWriter(1100, Allocator.Temp);
			var customMessagingManager = NetworkManager.CustomMessagingManager;
			using (writer)
			{
				writer.WriteValueSafe(messageContent);
				if (IsServer)
				{
					// This is a server-only method that will broadcast the named message.
					// Caution: Invoking this method on a client will throw an exception!
					customMessagingManager.SendNamedMessageToAll(kMessageName, writer);
				}
				else
				{
					// This is a client or server method that sends a named message to one target destination
					// (client to server or server to client)
					customMessagingManager.SendNamedMessage(kMessageName, NetworkManager.ServerClientId, writer);
				}
			}
		}
	}
}
