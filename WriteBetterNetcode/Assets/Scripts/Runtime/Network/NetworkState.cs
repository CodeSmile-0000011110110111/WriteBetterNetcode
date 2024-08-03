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

		public FSM m_Statemachine = new(nameof(NetworkState));
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
					StartClient();
					break;
				case NetworkRole.Host:
					StartHost();
					break;
				case NetworkRole.Server:
					StartServer();
					break;
				case NetworkRole.None:
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void Update() => m_Statemachine.Update();

		private void SetupStatemachine()
		{
			m_Statemachine.OnStateChange += args =>
				Debug.LogWarning($"{m_Statemachine} change: {args.PreviousState} to {args.ActiveState}");

			m_Statemachine.AllowMultipleStateChanges = true;
			m_NetworkRole = m_Statemachine.Vars.DefineInt("NetworkRole");

			var states = FSM.S(Enum.GetNames(typeof(State)));
			m_Statemachine.WithStates(states);

			var initState = states[(Int32)State.Initializing];
			var offlineState = states[(Int32)State.Offline];
			var serverStartingState = states[(Int32)State.ServerStarting];
			var serverStartedState = states[(Int32)State.ServerStarted];

			// Init state
			initState.WithTransitions(FSM.T("Goto Offline", offlineState)
				.WithConditions(new IsNetworkOffline()));

			// Shutdown state
			var stoppingState = states[(Int32)State.Stopping];
			stoppingState.WithTransitions(FSM.T("Network stopping", offlineState)
				.WithConditions(new IsNetworkOffline())
				.WithActions(FSM.SetValue(m_NetworkRole, (Int32)NetworkRole.None)));

			// Server States
			offlineState.WithTransitions(FSM.T("Start Server", serverStartingState)
				.WithConditions(FSM.IsEqual(m_NetworkRole, (Int32)NetworkRole.Server))
				.WithActions(new NetworkStart(NetworkRole.Server)));

			serverStartingState.WithTransitions(
				FSM.T("Server started", serverStartedState)
					.WithConditions(new IsLocalServerStarted()));

			serverStartedState.WithTransitions(FSM.T("Server stopping", stoppingState)
				.WithConditions(FSM.OR(FSM.NOT(new IsLocalServerStarted()), FSM.IsEqual(m_NetworkRole, (Int32)NetworkRole.None)))
				.WithActions(new NetworkStop()));

			// Host States (for all intents and purposes of network state, the host is the server)
			offlineState.WithTransitions(FSM.T("Start Host", serverStartingState)
				.WithConditions(FSM.IsEqual(m_NetworkRole, (Int32)NetworkRole.Host))
				.WithActions(new NetworkStart(NetworkRole.Host)));

			// Client States
			var clientStartingState = states[(Int32)State.ClientStarting];
			var clientStartedState = states[(Int32)State.ClientStarted];
			var clientConnectedState = states[(Int32)State.ClientConnected];
			var clientDisconnectedState = states[(Int32)State.ClientDisconnected];

			var clientStopTransition = FSM.T("Client stopping", stoppingState)
				.WithConditions(FSM.OR(FSM.NOT(new IsLocalClientStarted()), FSM.IsEqual(m_NetworkRole, (Int32)NetworkRole.None)))
				.WithActions(new NetworkStop());

			offlineState.WithTransitions(FSM.T("Start Client", clientStartingState)
				.WithConditions(FSM.IsEqual(m_NetworkRole, (Int32)NetworkRole.Client))
				.WithActions(new NetworkStart(NetworkRole.Client)));

			clientStartingState.WithTransitions(FSM.T("Client started", clientStartedState)
				.WithConditions(new IsLocalClientStarted()));

			clientStartedState.WithTransitions(FSM.T("Client connected", clientConnectedState)
				.WithConditions(new IsLocalClientConnected()), clientStopTransition);

			clientConnectedState.WithTransitions(FSM.T("Client disconnected", clientDisconnectedState)
				.WithConditions(FSM.NOT(new IsLocalClientConnected())), clientStopTransition);

			clientDisconnectedState.WithTransitions(clientStopTransition);
		}

		public void StartServer() => m_NetworkRole.IntValue = (Int32)NetworkRole.Server;
		public void StartHost() => m_NetworkRole.IntValue = (Int32)NetworkRole.Host;
		public void StartClient() => m_NetworkRole.IntValue = (Int32)NetworkRole.Client;
		public void StopNetwork() => m_NetworkRole.IntValue = (int)NetworkRole.None;

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
