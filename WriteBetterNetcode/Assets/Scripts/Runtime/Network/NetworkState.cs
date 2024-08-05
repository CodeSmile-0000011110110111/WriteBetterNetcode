// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine.Actions;
using CodeSmile.Statemachine.Conditions;
using CodeSmile.Statemachine.Netcode;
using CodeSmile.Statemachine.Netcode.Actions;
using CodeSmile.Statemachine.Netcode.Conditions;
using CodeSmile.Statemachine.Services.Authentication.Actions;
using CodeSmile.Statemachine.Services.Authentication.Conditions;
using System;
using System.IO;
using System.Linq;
using Unity.Multiplayer.Playmode;
using UnityEditor;
using UnityEngine;
using FSM = CodeSmile.Statemachine.FSM;

namespace CodeSmile.BetterNetcode.Network
{
	[Serializable]
	public struct ServerConfig
	{
		public Int32 MaxClients;
	}

	public class NetworkState : MonoBehaviour
	{
		public enum State
		{
			Initializing,
			Offline,

			ServerStarting,
			ServerStarted,

			ClientStarting,
			ClientStarted,
			ClientConnected,
			ClientDisconnected,

			Stopping,
		}

		// [SerializeField] private NetworkConfig m_ServerConfig = new() { Role = NetworkRole.Server };
		// [SerializeField] private NetworkConfig m_HostConfig = new() { Role = NetworkRole.Host };
		// [SerializeField] private NetworkConfig m_ClientConfig = new() { Role = NetworkRole.Client };

		public FSM m_Statemachine;
		private FSM.IntVariable m_NetworkRoleVar;
		private FSM.StructVariable<TransportConfig> m_TransportConfigVar;

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
			m_Statemachine = new FSM(new String(nameof(NetworkState))).WithStates(Enum.GetNames(typeof(State)));
			m_Statemachine.AllowMultipleStateChanges = true;
			m_Statemachine.OnStateChange += args =>
				Debug.LogWarning($"{m_Statemachine} change: {args.PreviousState} to {args.ActiveState}");

			m_NetworkRoleVar = m_Statemachine.Vars.DefineInt(nameof(NetworkRole));
			m_TransportConfigVar = m_Statemachine.Vars.DefineStruct<TransportConfig>(nameof(TransportConfig));

			var states = m_Statemachine.States;
			var initState = states[(Int32)State.Initializing];
			var offlineState = states[(Int32)State.Offline];
			var serverStartingState = states[(Int32)State.ServerStarting];
			var serverStartedState = states[(Int32)State.ServerStarted];
			var clientStartingState = states[(Int32)State.ClientStarting];
			var clientStartedState = states[(Int32)State.ClientStarted];
			var clientConnectedState = states[(Int32)State.ClientConnected];
			var clientDisconnectedState = states[(Int32)State.ClientDisconnected];
			var stoppingState = states[(Int32)State.Stopping];

			// Init state
			initState.AddTransition("Init Complete")
				.To(offlineState)
				.WithConditions(new IsNetworkOffline())
				.WithActions(new UnityServicesInit());

			// Offline state
			offlineState.AddTransition("SignInAnonymously")
				.WithConditions(new IsNotSignedIn(),
					new IsNotEqual(m_NetworkRoleVar, (Int32)NetworkRole.None))
				.WithActions(new SignInAnonymously());
			offlineState.AddTransition("Start Server")
				.To(serverStartingState)
				.WithConditions(new IsSignedIn(),
					new IsEqual(m_NetworkRoleVar, (Int32)NetworkRole.Server))
				.WithActions(new ApplyTransportConfig(m_TransportConfigVar),
					new NetworkStart(NetworkRole.Server));
			offlineState.AddTransition("Start Host")
				.To(serverStartingState)
				.WithConditions(new IsSignedIn(),
					new IsEqual(m_NetworkRoleVar, (Int32)NetworkRole.Host))
				.WithActions(new ApplyTransportConfig(m_TransportConfigVar),
					new NetworkStart(NetworkRole.Host));
			offlineState.AddTransition("Start Client")
				.To(clientStartingState)
				.WithConditions(new IsSignedIn(),
					new IsEqual(m_NetworkRoleVar, (Int32)NetworkRole.Client))
				.WithActions(new ApplyTransportConfig(m_TransportConfigVar),
					new NetworkStart(NetworkRole.Client));

			// Server States
			serverStartingState.AddTransition("Server started")
				.To(serverStartedState)
				.WithConditions(new IsLocalServerStarted());
			serverStartedState.AddTransition("Server stopping")
				.To(stoppingState)
				.WithConditions(FSM.OR(FSM.NOT(new IsLocalServerStarted()),
					new IsEqual(m_NetworkRoleVar, (Int32)NetworkRole.None)))
				.WithActions(new NetworkStop());

			// Client States
			clientStartingState.AddTransition("Client started")
				.To(clientStartedState)
				.WithConditions(new IsLocalClientStarted());
			clientStartedState.AddTransition("Client connected")
				.To(clientConnectedState)
				.WithConditions(new IsLocalClientConnected());
			clientConnectedState.AddTransition("Client disconnected")
				.To(clientDisconnectedState)
				.WithConditions(FSM.NOT(new IsLocalClientConnected()));

			// Stopping state
			stoppingState.AddTransition("Network stopping")
				.To(offlineState)
				.WithConditions(new IsNetworkOffline())
				.WithActions(new Assign(m_NetworkRoleVar, (Int32)NetworkRole.None));

			// stop transition gets added to multiple states
			FSM.CreateTransition("Client stopping")
				.To(stoppingState)
				.AddToStates(clientStartedState, clientConnectedState, clientDisconnectedState)
				.WithConditions(FSM.OR(FSM.NOT(new IsLocalClientStarted()),
					new IsEqual(m_NetworkRoleVar, (Int32)NetworkRole.None)))
				.WithActions(new NetworkStop());
		}

		public void RequestStartServer()
		{
			m_NetworkRoleVar.Value = (Int32)NetworkRole.Server;
			m_TransportConfigVar.Value = new TransportConfig
			{
				Role = NetworkRole.Server,
			};
		}

		public void RequestStartHost() => m_NetworkRoleVar.Value = (Int32)NetworkRole.Host;
		public void RequestStartClient() => m_NetworkRoleVar.Value = (Int32)NetworkRole.Client;
		public void RequestStopNetwork() => m_NetworkRoleVar.Value = (Int32)NetworkRole.None;

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

		public void StartNetworking(TransportConfig config)
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
