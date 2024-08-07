// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Statemachine;
using CodeSmile.Statemachine.Actions;
using CodeSmile.Statemachine.Netcode;
using CodeSmile.Statemachine.Netcode.Actions;
using CodeSmile.Statemachine.Netcode.Conditions;
using CodeSmile.Statemachine.Services;
using CodeSmile.Statemachine.Services.Authentication.Actions;
using CodeSmile.Statemachine.Services.Authentication.Conditions;
using CodeSmile.Statemachine.Services.Relay.Actions;
using CodeSmile.Statemachine.Services.Relay.Conditions;
using CodeSmile.Statemachine.Variable.Actions;
using CodeSmile.Statemachine.Variable.Conditions;
using System;
using System.IO;
using System.Linq;
using Unity.Multiplayer.Playmode;
using UnityEditor;
using UnityEngine;
using FSM = CodeSmile.Statemachine.FSM;
using SetFalse = CodeSmile.Statemachine.Variable.Actions.SetFalse;

namespace CodeSmile.BetterNetcode.Network
{
	[Serializable]
	public struct ServerConfig
	{
		public Int32 MaxClients;
	}

	public class NetcodeState : MonoBehaviour
	{
		public enum State
		{
			Initializing,
			Offline,
			RelayStarting,
			NetworkStarting,
			ServerOnline,
			ClientOnline,
			ClientPlaying,
			NetworkStopping,
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
			m_Statemachine = new FSM($"{nameof(NetcodeState)}Machine").WithStates(Enum.GetNames(typeof(State)));
			m_Statemachine.AllowMultipleStateChanges = true;
			m_Statemachine.OnStateChange += args =>
				Debug.LogWarning($"[{Time.frameCount}] {m_Statemachine} change: {args.PreviousState} to {args.ActiveState}");

			m_NetcodeConfigVar = m_Statemachine.Vars.DefineStruct<NetcodeConfig>(nameof(NetcodeConfig));
			m_RelayConfigVar = m_Statemachine.Vars.DefineStruct<RelayConfig>(nameof(RelayConfig));
			m_TransportConfigVar = m_Statemachine.Vars.DefineStruct<TransportConfig>(nameof(TransportConfig));
			var relayInitOnceVar = m_Statemachine.Vars.DefineBool("Relay Init Once");

			var states = m_Statemachine.States;
			var initState = states[(Int32)State.Initializing];
			var offlineState = states[(Int32)State.Offline];
			var relayStartState = states[(Int32)State.RelayStarting];
			var networkStartState = states[(Int32)State.NetworkStarting];
			var serverOnlineState = states[(Int32)State.ServerOnline];
			var clientOnlineState = states[(Int32)State.ClientOnline];
			var clientPlayingState = states[(Int32)State.ClientPlaying];
			var networkStopState = states[(Int32)State.NetworkStopping];

			m_Statemachine.Logging = true;

			var resetNetcodeState = new GroupAction("ResetNetcodeState",
				new SetFalse(relayInitOnceVar),
				new SetNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None),
				new RelayClearAllocationData(m_RelayConfigVar));

			/* TODO (MAJOR ONES)
			 * - how to handle action exceptions?
			 *		transition => ToErrorState ?? (handle exception where it occurs)
			 *		statemachine => move to fixed error state with its own transitions? (sounds good, or too brute force?)
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
				.ToState(relayStartState)
				.WithConditions(
					new IsNotNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None),
					new IsRelayEnabled(m_RelayConfigVar));

			offlineState.AddTransition("Start w/o Relay")
				.ToState(networkStartState)
				.WithConditions(
					new IsNotNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None),
					FSM.NOT(new IsRelayEnabled(m_RelayConfigVar)));

			// Relay state
			relayStartState.AddTransition("Relay Alloc/Join")
				.WithConditions(new IsFalse(relayInitOnceVar))
				.WithActions(
					new SetTrue(relayInitOnceVar),
					new SignInAnonymously(),
					new RelayCreateOrJoinAllocation(m_NetcodeConfigVar, m_RelayConfigVar))
				.ToErrorState(offlineState)
				.WithErrorActions(resetNetcodeState);
			relayStartState.AddTransition("Relay Started")
				.ToState(networkStartState)
				.WithConditions(
					new IsSignedIn(),
					new IsRelayReady(m_RelayConfigVar));

			// NetworkStart state
			networkStartState.AddTransition("Network Starting")
				.WithConditions(new IsNotListening())
				.WithActions(
					new TransportSetup(m_NetcodeConfigVar, m_TransportConfigVar, m_RelayConfigVar),
					new NetworkStart(m_NetcodeConfigVar))
				.ToErrorState(offlineState)
				.WithErrorActions(resetNetcodeState);
			networkStartState.AddTransition("Server started")
				.ToState(serverOnlineState)
				.WithConditions(new IsLocalServerStarted());
			networkStartState.AddTransition("Client started")
				.ToState(clientOnlineState)
				.WithConditions(new IsLocalClientStarted());

			// Server States
			serverOnlineState.AddTransition("Server stopped")
				.ToState(networkStopState)
				.WithConditions(
					FSM.OR(new IsLocalServerStopped(),
					new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None)))
				.WithActions(new NetworkStop());

			// Client States
			clientOnlineState.AddTransition("Client connected")
				.ToState(clientPlayingState)
				.WithConditions(new IsLocalClientConnected());
			clientOnlineState.AddTransition("Client stopped")
				.ToState(networkStopState)
				.WithConditions(
					FSM.OR(new IsLocalClientStopped(),
					new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None)))
				.WithActions(new NetworkStop());

			clientPlayingState.AddTransition("Client disconnected or stopped")
				.ToState(networkStopState)
				.WithConditions(FSM.OR(
					new IsLocalClientDisconnected(),
					new IsLocalClientStopped(),
					new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None)))
				.WithActions(new NetworkStop());

			// Stopping state
			networkStopState.AddTransition("Network stopped")
				.ToState(offlineState)
				.WithConditions(new IsNetworkOffline())
				.WithActions(resetNetcodeState);
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

		public void RequestStartClient()
		{
			m_NetcodeConfigVar.Value = new NetcodeConfig { Role = NetcodeRole.Client };
			m_RelayConfigVar.Value = new RelayConfig
			{
				UseRelayService = true,
				MaxConnections = 4,
				JoinCode = "CDFGH78",
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
