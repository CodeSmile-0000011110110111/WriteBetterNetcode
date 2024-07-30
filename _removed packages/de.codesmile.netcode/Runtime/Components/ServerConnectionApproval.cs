// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;
using CodeSmile.Netcode.Extensions;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     Handles connection approval and keeps each client's connection payload for later access.
	///     Ensures clients with payloads exceeding the given size are refused (possible DOS attack).
	///     Accepts connection by default. Subclass and override HandleApprovalRequest to customize.
	/// </summary>
	[DisallowMultipleComponent]
	public class ServerConnectionApproval : MonoSingleton<ServerConnectionApproval>
	{
		[Tooltip("Keep this as low as possible. Client approval will fail if clients send more than this many bytes " +
		         "as payload to minimize the impact of 'large payloads' DOS attacks.")]
		[SerializeField] private Int32 m_MaxPayloadBytes = 512;

		private readonly Dictionary<UInt64, Byte[]> m_ClientPayloads = new();

		/// <summary>
		///     Current client's connection payloads. In case they may be needed later.
		/// </summary>
		public IReadOnlyDictionary<UInt64, Byte[]> ClientPayloads => m_ClientPayloads;

		private void OnEnable() => NetworkManagerExt.InvokeWhenSingletonReady(RegisterCallbacks);

		protected override void OnDestroy()
		{
			base.OnDestroy();
			UnregisterCallbacks();
		}

		private Boolean PayloadSizeTooBig(NetworkManager.ConnectionApprovalRequest request,
			NetworkManager.ConnectionApprovalResponse response)
		{
			var payloadLength = request.Payload.Length;
			var tooBig = payloadLength > m_MaxPayloadBytes;
			if (tooBig)
			{
				response.Approved = false;
				response.Reason = "payload too big";
				NetworkLog.LogWarning($"client {request.ClientNetworkId} payload size is too big: {payloadLength} bytes");
			}

			return tooBig;
		}

		private void RegisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.ConnectionApprovalCallback -= OnConnectionApprovalRequest;
				netMan.ConnectionApprovalCallback += OnConnectionApprovalRequest;
				netMan.OnClientDisconnectCallback += OnClientDisconnect;
			}
		}

		private void UnregisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.ConnectionApprovalCallback -= OnConnectionApprovalRequest;
				netMan.OnClientDisconnectCallback -= OnClientDisconnect;
			}
		}

		private void OnClientDisconnect(UInt64 clientId) => m_ClientPayloads.Remove(clientId);

		private void OnConnectionApprovalRequest(NetworkManager.ConnectionApprovalRequest request,
			NetworkManager.ConnectionApprovalResponse response)
		{
			if (PayloadSizeTooBig(request, response))
				return;

			SetClientPayload(request);
			HandleApprovalRequest(request, response);
		}

		/// <summary>
		///     Override this to implement your custom approval processing. Don't call base since that will always approve.
		/// </summary>
		/// <param name="request">connection request</param>
		/// <param name="response">connection response</param>
		protected virtual void HandleApprovalRequest(NetworkManager.ConnectionApprovalRequest request,
			NetworkManager.ConnectionApprovalResponse response)
		{
			response.CreatePlayerObject = true;
			response.Approved = true;
			response.Reason = $"{nameof(NetworkSessionState)} approves";
		}

		private void SetClientPayload(NetworkManager.ConnectionApprovalRequest request) =>
			m_ClientPayloads[request.ClientNetworkId] = request.Payload;
	}
}
