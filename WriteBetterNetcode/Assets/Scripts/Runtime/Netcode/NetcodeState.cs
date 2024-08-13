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
using CodeSmile.Statemachine.Services.Core.Actions;
using CodeSmile.Statemachine.Services.Relay.Actions;
using CodeSmile.Statemachine.Services.Relay.Conditions;
using CodeSmile.Statemachine.Variable.Actions;
using CodeSmile.Statemachine.Variable.Conditions;
using System;
using System.IO;
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

		private FSM m_Statemachine;
		private Var<NetcodeConfig> m_NetcodeConfigVar;
		private Var<RelayConfig> m_RelayConfigVar;
		private Var<TransportConfig> m_TransportConfigVar;

		private void Awake() => SetupStatemachine();

		private void Start()
		{
			m_Statemachine.Start();

			// TODO: remove this ...
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
		}

		private void Update() => m_Statemachine.Update();

		private void SetupStatemachine()
		{
			Debug.Log("Setup Statemachine");
			m_Statemachine = new FSM($"{nameof(NetcodeState)}Machine")
				.WithStates(Enum.GetNames(typeof(State)));

			m_Statemachine.AllowMultipleStateChanges = true;

			var states = m_Statemachine.States;
			var initState = states[(Int32)State.Initializing];
			var offlineState = states[(Int32)State.Offline];
			var relayStartState = states[(Int32)State.RelayStarting];
			var networkStartState = states[(Int32)State.NetworkStarting];
			var serverOnlineState = states[(Int32)State.ServerOnline];
			var clientOnlineState = states[(Int32)State.ClientOnline];
			var clientPlayingState = states[(Int32)State.ClientPlaying];
			var networkStopState = states[(Int32)State.NetworkStopping];

			m_NetcodeConfigVar = m_Statemachine.Vars.DefineVar<NetcodeConfig>();
			m_RelayConfigVar = m_Statemachine.Vars.DefineVar<RelayConfig>();
			m_TransportConfigVar = m_Statemachine.Vars.DefineVar<TransportConfig>();
			var relayInitOnceVar = m_Statemachine.Vars.DefineBool("RelayInitOnce");

			// for testing
			//m_Statemachine.Logging = true;
			m_Statemachine.OnStateChange += args =>
				Debug.Log($"[{Time.frameCount}] {m_Statemachine} changed to {args.ActiveState}");

			var resetNetcodeState = new CompoundAction("ResetNetcodeState",
				new SetFalse(relayInitOnceVar),
				new SetNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None),
				new RelayClearAllocationData(m_RelayConfigVar));

			/* TODO (MAJOR ONES)
			 * - forward events (eg join code available)
			 *		route through config object ... BUT: config is a struct
			 * - Var<T> allow classes?
			 * - correct error handling for compound actions etc because they call the non-catching T methods!
			 * - support client connection dis-approval (received client disconnect event with reason)
			 */

			// Init state
			initState.AddTransition("Init Completed")
				.ToState(offlineState)
				.WithConditions(new IsNetworkManagerSingletonAssigned());

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
					new UnityServicesInit(),
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

		public void RequestStartNetwork(NetcodeConfig netcodeConfig, TransportConfig transportConfig,
			RelayConfig relayConfig = default)
		{
			if (netcodeConfig.Role == NetcodeRole.None)
				throw new ArgumentException("cannot start network without NetcodeRole");

			m_NetcodeConfigVar.Value = netcodeConfig;
			m_TransportConfigVar.Value = transportConfig;
			m_RelayConfigVar.Value = relayConfig;
		}

		public void RequestStopNetwork()
		{
			var netcodeConfig = m_NetcodeConfigVar.Value;
			netcodeConfig.Role = NetcodeRole.None;
			m_NetcodeConfigVar.Value = netcodeConfig;
		}
	}
}
