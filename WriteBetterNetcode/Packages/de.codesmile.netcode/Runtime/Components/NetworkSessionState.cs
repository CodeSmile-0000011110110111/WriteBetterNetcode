// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Netcode.Extensions;
using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	/// To be removed --- has no real purpose other than logging network events.
	/// </summary>
	[DisallowMultipleComponent]
	public class NetworkSessionState : MonoBehaviour
	{
		private void OnEnable() => NetworkManagerExt.InvokeWhenSingletonReady(RegisterCallbacks);

		private void OnDisable() => UnregisterCallbacks();

		private void RegisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.OnServerStarted += OnServerStarted;
				netMan.OnServerStopped += OnServerStopped;
				netMan.OnClientStarted += OnClientStarted;
				netMan.OnClientStopped += OnClientStopped;
				netMan.OnConnectionEvent += OnConnectionEvent;
				netMan.OnTransportFailure += OnTransportFailure;
			}
		}

		private void UnregisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.OnServerStarted -= OnServerStarted;
				netMan.OnServerStopped -= OnServerStopped;
				netMan.OnClientStarted -= OnClientStarted;
				netMan.OnClientStopped -= OnClientStopped;
				netMan.OnConnectionEvent -= OnConnectionEvent;
				netMan.OnTransportFailure -= OnTransportFailure;
			}
		}

		private void OnServerStarted() => NetworkLog.LogInfo("=> Server Started");
		private void OnClientStarted() => NetworkLog.LogInfo("=> Client Started");

		private void OnServerStopped(Boolean isHost) =>
			NetworkLog.LogInfo($"=> {(isHost ? "Server (Host)" : "Server")} Stopped");

		private void OnClientStopped(Boolean isHost) =>
			NetworkLog.LogInfo($"=> {(isHost ? "Client (Host)" : "Client")} Stopped");

		private void OnConnectionEvent(NetworkManager netMan, ConnectionEventData data) =>
			NetworkLog.LogInfo($"=> Connection Event: {data.EventType}, clientId={data.ClientId}");

		private void OnTransportFailure() => Debug.LogWarning("=> TRANSPORT FAILURE");
	}
}
