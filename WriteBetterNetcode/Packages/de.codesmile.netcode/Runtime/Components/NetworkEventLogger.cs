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
	public class NetworkEventLogger : MonoBehaviour
	{
		private Boolean m_ListeningState;
		private Boolean m_ShutdownInProgressState;
		private String LogPrefix => $"[{nameof(NetworkEventLogger)}] ";

		private void Awake() => NetworkManagerExt.InvokeWhenSingletonReady(() =>
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
			net.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
		});

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
				net.OnSessionOwnerPromoted -= OnSessionOwnerPromoted;
			}
		}

		private void Update()
		{
			var net = NetworkManager.Singleton;
			if (net != null)
			{
				if (net.IsListening != m_ListeningState)
				{
					m_ListeningState = net.IsListening;
					Debug.Log($"{LogPrefix}NetworkManager.IsListening = {net.IsListening}");
				}
				if (net.ShutdownInProgress != m_ShutdownInProgressState)
				{
					m_ShutdownInProgressState = net.ShutdownInProgress;
					Debug.Log($"{LogPrefix}NetworkManager.ShutdownInProgress = {net.ShutdownInProgress}");
				}
			}
		}

		private void LogSingletonReady() => Debug.Log($"{LogPrefix}NetworkManager.OnSingletonReady");
		private void OnServerStarted() => Debug.Log($"{LogPrefix}OnServerStarted()");
		private void OnServerStopped(Boolean isHost) => Debug.Log($"{LogPrefix}OnServerStopped(isHost: {isHost})");
		private void OnClientStarted() => Debug.Log($"{LogPrefix}OnClientStarted()");
		private void OnClientStopped(Boolean isHost) => Debug.Log($"{LogPrefix}OnClientStopped(isHost: {isHost})");
		private void OnClientConnected(UInt64 clientId) => Debug.Log($"{LogPrefix}OnClientConnected(clientId: {clientId})");
		private void OnClientDisconnect(UInt64 clientId) => Debug.Log($"{LogPrefix}OnClientDisconnect(clientId: {clientId})");

		private void OnConnectionEvent(NetworkManager net, ConnectionEventData data) => Debug.Log(
			$"{LogPrefix}OnConnectionEvent(NetworkManager: {net}, ConnectionEvent: {data.EventType} for clientId: {data.ClientId})");

		private void OnTransportFailure() => Debug.Log($"{LogPrefix}OnTransportFailure()");

		private void OnSessionOwnerPromoted(UInt64 sessionownerpromoted) =>
			Debug.Log($"{LogPrefix}OnSessionOwnerPromoted(sessionownerpromoted: {sessionownerpromoted})");
	}
}
