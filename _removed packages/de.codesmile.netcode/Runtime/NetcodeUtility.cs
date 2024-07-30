// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Netcode.Extensions;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode
{
	/// <summary>
	///     Launching Server/Host/Client with optional Relay allocation.
	///     Set the static public properties before calling the Server/Host/Client methods.
	/// </summary>
	/// <remarks>
	///     CAUTION: This may be redesigned or removed. It starts a session and has relay support.
	///     But moving forward it seems better to separate this into individual scripts, if not components,
	///     like: NetworkStart, NetworkRelay, NetworkAuthenticate ...
	/// </remarks>
	public static partial class NetcodeUtility
	{
		public static UnityTransport Transport => NetworkManager.Singleton.GetTransport();

		/// <summary>
		///     Start an isolated network session as host, disallowing incoming connections.
		/// </summary>
		public static async Task StartOffline()
		{
			// port 0 prevents incoming connections
			Transport.SetConnectionData("127.0.0.1", 0, "127.0.0.1");
			UseRelayService = false;

			await StartWithMode(NetworkMode.Host);
		}

		/// <summary>
		///     Start a network session as Server. Set UseRelayService before calling this to enable relay.
		/// </summary>
		public static async Task StartServer() => await StartWithMode(NetworkMode.Server);

		/// <summary>
		///     Start a network session as Host. Set UseRelayService before calling this to enable relay.
		/// </summary>
		public static async Task StartHost() => await StartWithMode(NetworkMode.Host);

		/// <summary>
		///     Start a network session as Client. Set UseRelayService and RelayJoinCode before calling this to enable relay.
		/// </summary>
		public static async Task StartClient() => await StartWithMode(NetworkMode.Client);

		public static void Disconnect()
		{
			var networkManager = NetworkManager.Singleton;
			if (networkManager != null && networkManager.IsListening)
				networkManager.Shutdown();
		}

		// TODO: use the NetworkConfig class
		private static async Task StartWithMode(NetworkMode mode)
		{
			var networkManager = NetworkManager.Singleton;
			if (networkManager == null)
				throw new NullReferenceException("NetworkManager.Singleton is null");

			try
			{
				if (UseRelayService)
				{
					if (mode == NetworkMode.Server || mode == NetworkMode.Host)
						RelayJoinCode = await AcquireRelayJoinCode(RelayMaxConnections, RelayConnectionType);
					else
						await JoinWithRelayCode(RelayJoinCode);
				}

				if (mode == NetworkMode.Server)
					networkManager.StartServer();
				else if (mode == NetworkMode.Host)
					networkManager.StartHost();
				else
					networkManager.StartClient();
			}
			catch (Exception e)
			{
				// TODO: conform to DSA
				// Confirm if the exception is an AuthenticationException, after every failed sign in attempt.
				// 	Confirm if the Notifications field isn't null.
				// 	Display the notifications to the player.

				Debug.LogError($"Start {mode}: {e}");
				throw;
			}
		}

		private static async Task AuthenticateAnonymously()
		{
			if (AuthenticationService.Instance.IsSignedIn == false)
			{
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
				Debug.LogWarning(AuthenticationService.Instance.PlayerInfo);
			}

			// TODO: conform to DSA
			// Confirm the LastNotificationDate field is not null after every successful sign in, meaning there are notifications available for that player.
			// 	Confirm if the LastNotificationDate is greater than the value you stored for the last notification the player read.
			// 	Retrieve the player's notifications by calling the GetNotificationsAsync method.
			// 	Display the notifications to the player. (The notifications are available in the return value of the GetNotificationsAsync method and are also cached in the Notifications field.)
			// When the player reads a notification you must store the greater value between that notification's CreatedAt value and the stored value for the player's last read notification.

			// var notifications = AuthenticationService.Instance.Notifications;
			// if (notifications == null || notifications.Count == 0)
			// 	Debug.Log("No Authentification notifications");
			// else
			// {
			// 	foreach (var note in notifications)
			// 		Debug.LogWarning($"Auth Notification: {note}");
			// }
		}

		private static async Task<String> AcquireRelayJoinCode(Int32 maxConnections, String connectionType)
		{
			await AuthenticateAnonymously();

			var createAlloc = await RelayService.Instance.CreateAllocationAsync(maxConnections);
			Transport.SetRelayServerData(new RelayServerData(createAlloc, connectionType));
			var joinCode = await RelayService.Instance.GetJoinCodeAsync(createAlloc.AllocationId);

			Debug.Log($"Relay join code is: {joinCode}");
			return joinCode;
		}

		private static async Task JoinWithRelayCode(String joinCode)
		{
			await AuthenticateAnonymously();

			Debug.Log($"Joining Relay with code: {joinCode}");
			var joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
			Transport.SetRelayServerData(new RelayServerData(joinAlloc, RelayConnectionType));
		}

		internal enum NetworkMode
		{
			Server,
			Host,
			Client,
		}
	}
}
