﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using System.Text;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Netcode
{
	/// <summary>
	///     Will log all NetworkManager events. Best to add this component on the same object as the NetworkManager.
	/// </summary>
	/// <remarks>
	///     The component is only active if DEBUG or DEVELOPMENT_BUILD symbols are defined.
	/// </remarks>
	[DisallowMultipleComponent]
	public sealed class NetworkEventLogger : MonoBehaviour
	{
		[SerializeField] private Boolean m_LogPropertyChanges = true;
		[SerializeField] private Boolean m_LogSceneEvents = true;
		[SerializeField] private Color32 m_LogTextColor = Color.gray;

		private Boolean m_ListeningState;
		private Boolean m_ShutdownInProgressState;
		private Boolean m_IsClientState;
		private Boolean m_IsConnectedClientState;
		private Boolean m_IsApprovedState;
		private Boolean m_IsHostState;
		private Boolean m_IsServerState;

		private void Awake()
		{
#if !UNITY_EDITOR && !DEBUG && !DEVELOPMENT_BUILD
			Destroy(this);
#else
			var net = GetComponent<NetworkManager>();
			if (net == null)
				throw new NullReferenceException($"NetworkManager component not found on object: {gameObject.name}");

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
#endif
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

				var sceneManager = net.SceneManager;
				if (sceneManager != null)
					sceneManager.OnSceneEvent -= OnSceneEvent;
			}
		}

		private void Update()
		{
			if (m_LogPropertyChanges)
				LogPropertyChanges();
		}

		private void RegisterSceneEventCallback(Boolean register)
		{
			var sceneManager = NetworkManager.Singleton?.SceneManager;
			if (sceneManager != null)
			{
				sceneManager.OnSceneEvent -= OnSceneEvent;
				if (m_LogSceneEvents && register)
					sceneManager.OnSceneEvent += OnSceneEvent;
			}
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

		private void OnServerStarted()
		{
			Log("OnServerStarted()");
			RegisterSceneEventCallback(true);
		}

		private void OnServerStopped(Boolean isHost)
		{
			Log($"OnServerStopped(isHost: {isHost})");
			RegisterSceneEventCallback(false);
		}

		private void OnClientStarted()
		{
			Log("OnClientStarted()");
			RegisterSceneEventCallback(true);
		}

		private void OnClientStopped(Boolean isHost)
		{
			Log($"OnClientStopped(isHost: {isHost})");
			RegisterSceneEventCallback(false);
		}

		private void OnClientConnected(UInt64 clientId) => Log($"OnClientConnected(clientId: {clientId})");
		private void OnClientDisconnect(UInt64 clientId) => Log($"OnClientDisconnect(clientId: {clientId})");

		private void OnConnectionEvent(NetworkManager net, ConnectionEventData data) => Log(
			$"OnConnectionEvent({data.EventType}: ClientId = {data.ClientId})");

		private void OnTransportFailure() => Log("OnTransportFailure()");

		private void OnSessionOwnerPromoted(UInt64 sessionownerpromoted) =>
			Log($"OnSessionOwnerPromoted(sessionownerpromoted: {sessionownerpromoted})");

		private void OnSceneEvent(SceneEvent sceneEvent)
		{
			var completed = String.Empty;
			var timedOut = String.Empty;

			var completedCount = sceneEvent.ClientsThatCompleted?.Count;
			var timedOutCount = sceneEvent.ClientsThatTimedOut?.Count;

			if (completedCount > 0)
			{
				var first = true;
				var sb = new StringBuilder();
				foreach (var clientId in sceneEvent.ClientsThatCompleted)
					sb.Append($"{(first ? "" : ", ")}{clientId}");

				completed = $"(completed clientIDs: {sb})";
			}

			if (timedOutCount > 0)
			{
				var first = true;
				var sb = new StringBuilder();
				foreach (var clientId in sceneEvent.ClientsThatTimedOut)
					sb.Append($"{(first ? "" : ", ")}{clientId}");

				timedOut = $"<color=red>(TIMEOUT clientIDs: {sb})<color>";
			}

			var role = sceneEvent.ClientId == 0 ? "Server" : $"Client {sceneEvent.ClientId}";
			Log($"{role} Scene: {sceneEvent.SceneEventType} {sceneEvent.SceneName} ({sceneEvent.LoadSceneMode}) " +
			    $"{completed} {timedOut}");
		}

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

			var color = $"<color=#{m_LogTextColor.r:x02}{m_LogTextColor.g:x02}{m_LogTextColor.b:x02}>";
			Debug.Log($"{color}[L:{localTick}|S:{serverTick}|F:{frameCount}] {message}</color>");
		}

#if UNITY_EDITOR
		private void Reset()
		{
			var net = GetComponent<NetworkManager>();
			if (net == null)
			{
				Debug.LogWarning($"{nameof(NetworkEventLogger)} must be added to the NetworkManager object!");
				StartCoroutine(DestroyComponentAfterDelay());
			}
		}

		private IEnumerator DestroyComponentAfterDelay()
		{
			yield return null;

			DestroyImmediate(this);
		}
#endif
	}
}
