// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;
using CodeSmile.Netcode.Extensions;
using CodeSmile.SceneTools;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
#if UNITY_SERVER
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
#endif

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     (Dedicated Server build only) Starts the dedicated server session.
	///     Sets the server listen address to listen to all incoming connections. Disables relay, even if enabled.
	/// </summary>
	[DisallowMultipleComponent]
	public class ServerHeadlessAutostart : OneTimeTaskBehaviour
	{
		[SerializeField] private UInt16 m_Port = 7778;

// disable: async method lacks 'await' when not building Server
#pragma warning disable 1998
		private async void Start()
		{
#if UNITY_SERVER
			await Autostart();
#endif

			TaskPerformed();
		}

		private async Task Autostart()
		{
			Debug.Log("Dedicated Server autostart ...");

			// Dedicated server should not use relay
			NetcodeUtility.UseRelayService = false;
			SceneAutoLoader.DestroyAll();

			var transport = NetworkManager.Singleton.GetTransport();
			var listenAddress = transport.ConnectionData.IsIpv6 ? "::" : "0.0.0.0";
			transport.SetConnectionData("127.0.0.1", m_Port, listenAddress);

			await NetcodeUtility.StartServer();
		}
	}
}
