// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine;
using CodeSmile.Statemachine.Netcode;
using CodeSmile.Statemachine.Netcode.Actions;
using CodeSmile.Statemachine.Netcode.Conditions;
using System;
using System.IO;
using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.BetterNetcode.Network
{
	[Serializable]
	public struct NetworkConfig
	{
		[HideInInspector] public NetworkRole Role;
		public UnityTransport.ConnectionAddressData AddressData;
		//public RelayConfig RelayConfig;
		public Boolean UseWebSockets;
		public Boolean UseEncryption;

		public override String ToString() => $"NetworkConfig(Role={Role}, Address={AddressData.Address}:{AddressData.Port}, " +
		                                     $"Listen={AddressData.ServerListenAddress}, WebSockets={UseWebSockets}, Encryption={UseEncryption})";
	}

	[Serializable]
	public struct ServerConfig
	{
		public Int32 MaxPlayers;
	}

	[Serializable]
	public struct RelayConfig
	{
		public Boolean UseRelayService;
		public String RelayJoinCode;
	}

	public class NetworkState : MonoBehaviour
	{
		public enum State
		{
			Initializing,
			Offline,
			Configure,
			Stopping,

			ServerStarting,
			ServerStarted,

			ClientStarting,
			ClientStarted,
			ClientConnected,
			ClientDisconnected,
		}

		[SerializeField] private NetworkConfig m_ServerConfig = new() { Role = NetworkRole.Server };
		[SerializeField] private NetworkConfig m_HostConfig = new() { Role = NetworkRole.Host };
		[SerializeField] private NetworkConfig m_ClientConfig = new() { Role = NetworkRole.Client };

		public FSM m_Statemachine;
		private FSM.Variable m_NetworkRole;

		private void Awake() => SetupStatemachine();

		private void Start()
		{
			m_Statemachine.Start();

			try
			{
				var puml = m_Statemachine.ToPlantUml();
				File.WriteAllText($"{Application.dataPath}/../../PlantUML Diagrams/{GetType().FullName}.puml",
					$"@startuml\n\n!theme blueprint\nhide empty description\n\n{puml}\n\n@enduml");
			}
			catch (Exception e)
			{
				Debug.LogWarning(e);
			}

			m_Statemachine.Update();

			var mppmRole = GetNetworkRoleFromMppmTags();
			Debug.LogWarning("Network Role: " + mppmRole);

			switch (mppmRole)
			{
				case NetworkRole.Client:
					RequestStartClient();
					break;
				case NetworkRole.Host:
					RequestStartHost();
					break;
				case NetworkRole.Server:
					RequestStartServer();
					break;
				case NetworkRole.None:
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void Update() => m_Statemachine.Update();

		private void SetupStatemachine()
		{
			m_Statemachine = new FSM(new(nameof(NetworkState))).WithStates(Enum.GetNames(typeof(State)));
			m_Statemachine.AllowMultipleStateChanges = true;
			m_Statemachine.OnStateChange += args =>
				Debug.LogWarning($"{m_Statemachine} change: {args.PreviousState} to {args.ActiveState}");

			m_NetworkRole = m_Statemachine.Vars.DefineInt("NetworkRole");

			var states = m_Statemachine.States;
			var initState = states[(Int32)State.Initializing];
			var offlineState = states[(Int32)State.Offline];
			var serverStartingState = states[(Int32)State.ServerStarting];
			var serverStartedState = states[(Int32)State.ServerStarted];
			var stoppingState = states[(Int32)State.Stopping];

			// Init & stopping states
			initState.AddTransition("Init Complete").To(offlineState)
				.WithConditions(new IsNetworkOffline());
			stoppingState.AddTransition("Network stopping").To(offlineState)
				.WithConditions(new IsNetworkOffline())
				.WithActions(FSM.SetVarValue(m_NetworkRole, (Int32)NetworkRole.None));

			// Server States
			offlineState.AddTransition("Start Server").To(serverStartingState)
				.WithConditions(FSM.IsVarEqual(m_NetworkRole, (Int32)NetworkRole.Server))
				.WithActions(new NetworkStart(NetworkRole.Server));
			serverStartingState.AddTransition("Server started").To(serverStartedState)
				.WithConditions(new IsLocalServerStarted());
			serverStartedState.AddTransition("Server stopping").To(stoppingState)
				.WithConditions(FSM.OR(
					FSM.NOT(new IsLocalServerStarted()),
					FSM.IsVarEqual(m_NetworkRole, (Int32)NetworkRole.None)))
				.WithActions(new NetworkStop());

			// Host States (for all intents and purposes of network state, the host is the server)
			offlineState.AddTransition("Start Host").To(serverStartingState)
				.WithConditions(FSM.IsVarEqual(m_NetworkRole, (Int32)NetworkRole.Host))
				.WithActions(new NetworkStart(NetworkRole.Host));

			// Client States
			var clientStartingState = states[(Int32)State.ClientStarting];
			var clientStartedState = states[(Int32)State.ClientStarted];
			var clientConnectedState = states[(Int32)State.ClientConnected];
			var clientDisconnectedState = states[(Int32)State.ClientDisconnected];

			offlineState.AddTransition("Start Client").To(clientStartingState)
				.WithConditions(FSM.IsVarEqual(m_NetworkRole, (Int32)NetworkRole.Client))
				.WithActions(new NetworkStart(NetworkRole.Client));
			clientStartingState.AddTransition("Client started").To(clientStartedState)
				.WithConditions(new IsLocalClientStarted());
			clientStartedState.AddTransition("Client connected").To(clientConnectedState)
				.WithConditions(new IsLocalClientConnected());
			clientConnectedState.AddTransition("Client disconnected").To(clientDisconnectedState)
				.WithConditions(FSM.NOT(new IsLocalClientConnected()));

			// stop transition gets added to multiple states
			FSM.CreateTransition("Client stopping").To(stoppingState)
				.AddToStates(clientStartedState, clientConnectedState, clientDisconnectedState)
				.WithConditions(FSM.OR(
					FSM.NOT(new IsLocalClientStarted()),
					FSM.IsVarEqual(m_NetworkRole, (Int32)NetworkRole.None)))
				.WithActions(new NetworkStop());
		}

		public void RequestStartServer() => m_NetworkRole.IntValue = (Int32)NetworkRole.Server;
		public void RequestStartHost() => m_NetworkRole.IntValue = (Int32)NetworkRole.Host;
		public void RequestStartClient() => m_NetworkRole.IntValue = (Int32)NetworkRole.Client;
		public void RequestStopNetwork() => m_NetworkRole.IntValue = (Int32)NetworkRole.None;

		private NetworkRole GetNetworkRoleFromMppmTags()
		{
#if UNITY_EDITOR
			var tags = CurrentPlayer.ReadOnlyTags();
			var roleCount = Enum.GetValues(typeof(NetworkRole)).Length;
			for (var r = 0; r < roleCount; r++)
				if (tags.Contains(((NetworkRole)r).ToString()))
					return (NetworkRole)r;
#endif
			return NetworkRole.None;
		}

		public void StartNetworking(NetworkConfig config)
		{
			/*
			var net = NetworkManager.Singleton;
			var transport = net.GetTransport();
			transport.ConnectionData = config.AddressData;
			transport.UseWebSockets = config.UseWebSockets;
			transport.UseEncryption = config.UseEncryption;
		*/
		}
	}
}
