// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Netcode.Extensions;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     Will log all NetworkManager events. Best to add this component on the same object as the NetworkManager.
	/// </summary>
	/// <remarks>
	///     The component is only active if DEBUG or DEVELOPMENT_BUILD symbols are defined.
	/// </remarks>
	public class NetworkEventLogger : MonoBehaviour
	{
		[SerializeField] private Boolean m_LogPropertyChanges = true;

		private Boolean m_ListeningState;
		private Boolean m_ShutdownInProgressState;
		private Boolean m_IsClientState;
		private Boolean m_IsConnectedClientState;
		private Boolean m_IsApprovedState;
		private Boolean m_IsHostState;
		private Boolean m_IsServerState;

		private void Awake()
		{
#if !DEBUG && !DEVELOPMENT_BUILD
			Destroy(this);
			return;
#endif

			NetworkManagerExt.InvokeWhenSingletonReady(() =>
			{
				LogSingletonReady();

				var net = NetworkManager.Singleton;
				m_ListeningState = net.IsListening;
				m_ShutdownInProgressState = net.ShutdownInProgress;

				net.OnServerStarted += OnServerStarted;
				net.OnServerStopped += OnServerStopped;
				net.OnClientStarted += OnClientStarted;
				net.OnClientStopped += OnClientStopped;
				net.OnClientConnectedCallback += OnClientConnected;
				net.OnClientDisconnectCallback += OnClientDisconnect;
				net.OnConnectionEvent += OnConnectionEvent;
				net.OnTransportFailure += OnTransportFailure;

				// Netcode 2.0+
				//net.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
			});
		}

		private void OnDestroy()
		{
			var net = NetworkManager.Singleton;
			if (net != null)
			{
				net.OnServerStarted -= OnServerStarted;
				net.OnServerStopped -= OnServerStopped;
				net.OnClientStarted -= OnClientStarted;
				net.OnClientStopped -= OnClientStopped;
				net.OnClientConnectedCallback -= OnClientConnected;
				net.OnClientDisconnectCallback -= OnClientDisconnect;
				net.OnConnectionEvent -= OnConnectionEvent;
				net.OnTransportFailure -= OnTransportFailure;
				//net.OnSessionOwnerPromoted -= OnSessionOwnerPromoted;
			}
		}

		private void Update()
		{
			if (m_LogPropertyChanges)
				LogPropertyChanges();
		}

		private void LogPropertyChanges()
		{
			var net = NetworkManager.Singleton;
			if (net != null)
			{
				if (net.IsListening != m_ListeningState)
				{
					m_ListeningState = net.IsListening;
					Log($"IsListening = {net.IsListening}");
				}
				if (net.ShutdownInProgress != m_ShutdownInProgressState)
				{
					m_ShutdownInProgressState = net.ShutdownInProgress;
					Log($"ShutdownInProgress = {net.ShutdownInProgress}");
				}
				if (net.IsClient != m_IsClientState)
				{
					m_IsClientState = net.IsClient;
					Log($"IsClient = {net.IsClient}");
				}
				if (net.IsHost != m_IsHostState)
				{
					m_IsHostState = net.IsHost;
					Log($"IsHost = {net.IsHost}");
				}
				if (net.IsServer != m_IsServerState)
				{
					m_IsServerState = net.IsServer;
					Log($"IsServer = {net.IsServer}");
				}
				if (net.IsApproved != m_IsApprovedState)
				{
					m_IsApprovedState = net.IsApproved;
					Log($"IsApproved = {net.IsApproved}");
				}
				if (net.IsConnectedClient != m_IsConnectedClientState)
				{
					m_IsConnectedClientState = net.IsConnectedClient;
					Log($"IsConnectedClient = {net.IsConnectedClient}");
				}
			}
		}

		private void LogSingletonReady() => Log("NetworkManager.OnSingletonReady");
		private void OnServerStarted() => Log("OnServerStarted()");
		private void OnServerStopped(Boolean isHost) => Log($"OnServerStopped(isHost: {isHost})");
		private void OnClientStarted() => Log("OnClientStarted()");
		private void OnClientStopped(Boolean isHost) => Log($"OnClientStopped(isHost: {isHost})");
		private void OnClientConnected(UInt64 clientId) => Log($"OnClientConnected(clientId: {clientId})");
		private void OnClientDisconnect(UInt64 clientId) => Log($"OnClientDisconnect(clientId: {clientId})");

		private void OnConnectionEvent(NetworkManager net, ConnectionEventData data) => Log(
			$"OnConnectionEvent({data.EventType}: ClientId = {data.ClientId})");

		private void OnTransportFailure() => Log("OnTransportFailure()");

		private void OnSessionOwnerPromoted(UInt64 sessionownerpromoted) =>
			Log($"OnSessionOwnerPromoted(sessionownerpromoted: {sessionownerpromoted})");

		private void Log(String message)
		{
			var localTick = -1;
			var serverTick = -1;
			var frameCount = Time.frameCount;

			var net = NetworkManager.Singleton;
			if (net)
			{
				localTick = net.LocalTime.Tick;
				serverTick = net.ServerTime.Tick;
			}

			Debug.Log($"[LT:{localTick}|ST:{serverTick}|FC:{frameCount}] {message}");
		}
	}
}
