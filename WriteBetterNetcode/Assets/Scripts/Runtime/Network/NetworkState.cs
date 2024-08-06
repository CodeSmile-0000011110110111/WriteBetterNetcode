// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine;
using CodeSmile.Statemachine.Netcode;
using CodeSmile.Statemachine.Netcode.Actions;
using CodeSmile.Statemachine.Netcode.Conditions;
using CodeSmile.Statemachine.Services;
using CodeSmile.Statemachine.Services.Authentication.Actions;
using CodeSmile.Statemachine.Services.Authentication.Conditions;
using CodeSmile.Statemachine.Services.Relay.Actions;
using CodeSmile.Statemachine.Services.Relay.Conditions;
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
			RelayAllocating,
			Starting,
			ServerStarted,
			ClientStarted,
			ClientConnected,
			Stopping,
		}

		// [SerializeField] private NetworkConfig m_ServerConfig = new() { Role = NetworkRole.Server };
		// [SerializeField] private NetworkConfig m_HostConfig = new() { Role = NetworkRole.Host };
		// [SerializeField] private NetworkConfig m_ClientConfig = new() { Role = NetworkRole.Client };

		public FSM m_Statemachine;
		private Var<NetcodeConfig> m_NetcodeConfigVar;
		private Var<RelayConfig> m_RelayConfigVar;
		private Var<TransportConfig> m_TransportConfigVar;

		private void Awake() => SetupStatemachine();

		private void Start()
		{
			m_Statemachine.Start().Update();

			try
			{
				var puml = m_Statemachine.ToPlantUml();
				File.WriteAllText($"{Application.dataPath}/../../PlantUML Diagrams/{GetType().FullName}.puml",
					$"@startuml\n\n!theme blueprint\nhide empty description\n\n{puml}\n\n@enduml");
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			var mppmRole = GetNetworkRoleFromMppmTags();
			//Debug.Log("Network Role: " + mppmRole);

			switch (mppmRole)
			{
				case NetcodeRole.Client:
					RequestStartClient();
					break;
				case NetcodeRole.Host:
					RequestStartHost();
					break;
				case NetcodeRole.Server:
					RequestStartServer();
					break;
				case NetcodeRole.None:
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
				Debug.LogWarning($"[{Time.frameCount}] {m_Statemachine} change: {args.PreviousState} to {args.ActiveState}");

			m_NetcodeConfigVar = m_Statemachine.Vars.DefineStruct<NetcodeConfig>(nameof(NetcodeConfig));
			m_RelayConfigVar = m_Statemachine.Vars.DefineStruct<RelayConfig>(nameof(RelayConfig));
			m_TransportConfigVar = m_Statemachine.Vars.DefineStruct<TransportConfig>(nameof(TransportConfig));

			var states = m_Statemachine.States;
			var initState = states[(Int32)State.Initializing];
			var offlineState = states[(Int32)State.Offline];
			var relayAllocState = states[(Int32)State.RelayAllocating];
			var startingState = states[(Int32)State.Starting];
			var serverStartedState = states[(Int32)State.ServerStarted];
			var clientStartedState = states[(Int32)State.ClientStarted];
			var clientConnectedState = states[(Int32)State.ClientConnected];
			var stoppingState = states[(Int32)State.Stopping];

			/* TODO (MAJOR ONES)
			 * - how to handle action exceptions?
			 *		transition => ToErrorState ?? (handle exception where it occurs)
			 *		state => OnErrorTransition ?? (hmmmm)
			 * - forward events (eg join code available)
			 *		route through config object ... BUT: config is a struct
			 * - Var<T> allow classes?
			 */

			// Init state
			initState.AddTransition("Init Complete")
				.ToState(offlineState)
				.WithConditions(new IsNetworkOffline())
				.WithActions(new UnityServicesInit());

			// Offline state
			offlineState.AddTransition("Start with Relay")
				.ToState(relayAllocState)
				.WithConditions(
					FSM.NOT(new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None)),
					new IsRelayEnabled(m_RelayConfigVar))
				.WithActions(
					new SignInAnonymously(),
					new RelayCreateOrJoinAllocation(m_NetcodeConfigVar, m_RelayConfigVar));
			offlineState.AddTransition("Start w/o Relay")
				.ToState(startingState)
				.WithConditions(
					FSM.NOT(new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None)),
					FSM.NOT(new IsRelayEnabled(m_RelayConfigVar)))
				.WithActions(
					new TransportSetup(m_NetcodeConfigVar, m_TransportConfigVar, m_RelayConfigVar),
					new NetworkStart(m_NetcodeConfigVar));

			// Relay state
			// TODO: handle signIn & relay failing or throwing an exception!!
			relayAllocState.AddTransition("Relay Allocated")
				.ToState(startingState)
				.WithConditions(
					new IsSignedIn(),
					new IsRelayReady(m_RelayConfigVar))
				.WithActions(
					new TransportSetup(m_NetcodeConfigVar, m_TransportConfigVar, m_RelayConfigVar),
					new NetworkStart(m_NetcodeConfigVar));

			// Starting state
			startingState.AddTransition("Server started")
				.ToState(serverStartedState)
				.WithConditions(new IsLocalServerStarted());
			startingState.AddTransition("Client started")
				.ToState(clientStartedState)
				.WithConditions(new IsLocalClientStarted());

			// Server States
			serverStartedState.AddTransition("Server stopping")
				.ToState(stoppingState)
				.WithConditions(FSM.OR(
					FSM.NOT(new IsLocalServerStarted()),
					new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None)))
				.WithActions(new NetworkStop());

			// Client States
			clientStartedState.AddTransition("Client connected")
				.ToState(clientConnectedState)
				.WithConditions(new IsLocalClientConnected());
			clientConnectedState.AddTransition("Client disconnected")
				.ToState(stoppingState)
				.WithConditions(FSM.NOT(new IsLocalClientConnected()))
				.WithActions(new NetworkStop());

			// Stopping state
			stoppingState.AddTransition("Network stopping")
				.ToState(offlineState)
				.WithConditions(new IsNetworkOffline())
				.WithActions(new SetNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None));

			// stop transition gets added to multiple states
			FSM.CreateTransition("Client stopping")
				.ToState(stoppingState)
				.AddToStates(clientStartedState, clientConnectedState)
				.WithConditions(FSM.OR(
					FSM.NOT(new IsLocalClientStarted()),
					new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None)))
				.WithActions(new NetworkStop());
		}

		public void RequestStartServer()
		{
			m_NetcodeConfigVar.Value = new NetcodeConfig { Role = NetcodeRole.Server };
			m_RelayConfigVar.Value = new RelayConfig
			{
				UseRelayService = true,
				MaxConnections = 4,
			};
			m_TransportConfigVar.Value = new TransportConfig
			{
				Address = "127.0.0.1",
				Port = 7777,
				ServerListenAddress = "0.0.0.0",
				UseEncryption = false,
				UseWebSockets = false,
			};
		}

		public void RequestStartHost() => throw new NotImplementedException();
		public void RequestStartClient() => throw new NotImplementedException();
		public void RequestStopNetwork() => throw new NotImplementedException();

		private NetcodeRole GetNetworkRoleFromMppmTags()
		{
#if UNITY_EDITOR
			var tags = CurrentPlayer.ReadOnlyTags();
			var roleCount = Enum.GetValues(typeof(NetcodeRole)).Length;
			for (var r = 0; r < roleCount; r++)
				if (tags.Contains(((NetcodeRole)r).ToString()))
					return (NetcodeRole)r;
#endif
			return NetcodeRole.None;
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
