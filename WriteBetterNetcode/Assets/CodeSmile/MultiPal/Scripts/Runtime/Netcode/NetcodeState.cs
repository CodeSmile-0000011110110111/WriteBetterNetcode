// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.Statemachine;
using CodeSmile.Statemachine.Actions;
using CodeSmile.Statemachine.Netcode;
using CodeSmile.Statemachine.Netcode.Actions;
using CodeSmile.Statemachine.Netcode.Conditions;
using CodeSmile.Statemachine.Services.Authentication.Actions;
using CodeSmile.Statemachine.Services.Authentication.Conditions;
using CodeSmile.Statemachine.Services.Core.Actions;
using CodeSmile.Statemachine.Services.Relay.Actions;
using CodeSmile.Statemachine.Services.Relay.Conditions;
using CodeSmile.Statemachine.Variable.Actions;
using CodeSmile.Statemachine.Variable.Conditions;
using System;
using System.IO;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.MultiPal.Netcode
{
	[DisallowMultipleComponent]
	public sealed class NetcodeState : MonoBehaviour
	{
		public event Action<NetcodeRole> WentOffline;
		public event Action<NetcodeRole> WentOnline;
		public event Action<String> RelayJoinCodeAvailable;

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

		private FSM m_Statemachine;
		private Var<NetcodeConfig> m_NetcodeConfigVar;
		private Var<RelayConfig> m_RelayConfigVar;
		private Var<TransportConfig> m_TransportConfigVar;

		public Boolean IsOnline { get; private set; }

		private void Awake()
		{
			ComponentsRegistry.Set(this);

			WentOnline += OnWentOnline;
			WentOffline += OnWentOffline;

			SetupStatemachine();
		}

		private void OnDestroy()
		{
			WentOnline -= OnWentOnline;
			WentOffline -= OnWentOffline;
		}

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

		private void OnWentOnline(NetcodeRole role) => IsOnline = true;
		private void OnWentOffline(NetcodeRole role) => IsOnline = false;

		private void SetupStatemachine()
		{
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
			m_Statemachine.OnStateChange += args => Debug.Log($"<color=olive>[{Time.frameCount}] {m_Statemachine} changed " +
			                                                  $"from {args.PreviousState} to {args.ActiveState}</color>");

			var invokeWentOnline = new LambdaAction($"{nameof(WentOnline)}.Invoke",
				() => WentOnline?.Invoke(m_NetcodeConfigVar.Value.Role));
			var invokeWentOffline = new LambdaAction($"{nameof(WentOffline)}.Invoke",
				() => WentOffline?.Invoke(m_NetcodeConfigVar.Value.Role));
			var tryInvokeRelayJoinCodeAvailable = new LambdaAction(nameof(TryInvokeRelayJoinCodeAvailable),
				() => TryInvokeRelayJoinCodeAvailable());

			var resetNetcodeState = new CompoundAction("ResetNetcodeState",
				new SetFalse(relayInitOnceVar),
				new SetNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None),
				new RelayClearAllocationData(m_RelayConfigVar),
				invokeWentOffline);

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
					new IsRelayEnabled(m_RelayConfigVar))
				.WithActions(invokeWentOnline);

			offlineState.AddTransition("Start w/o Relay")
				.ToState(networkStartState)
				.WithConditions(
					new IsNotNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None),
					FSM.NOT(new IsRelayEnabled(m_RelayConfigVar)))
				.WithActions(invokeWentOnline);

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
					new IsRelayReady(m_RelayConfigVar))
				.WithActions(tryInvokeRelayJoinCodeAvailable);

			// NetworkStart state
			networkStartState.AddTransition("Cancel Net Start")
				.ToState(offlineState)
				.WithConditions(new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None))
				.WithActions(resetNetcodeState);

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

		private void TryInvokeRelayJoinCodeAvailable()
		{
			var role = m_NetcodeConfigVar.Value.Role;
			if (role == NetcodeRole.Server || role == NetcodeRole.Host)
			{
				var joinCode = m_RelayConfigVar.Value.JoinCode;
				if (String.IsNullOrEmpty(joinCode) == false)
					RelayJoinCodeAvailable?.Invoke(joinCode);
			}
		}

		public void RequestStart(NetcodeConfig netcodeConfig,
			TransportConfig transportConfig, RelayConfig relayConfig = default)
		{
			if (netcodeConfig.Role == NetcodeRole.None)
				throw new ArgumentException("cannot start without role");

			Debug.Log(netcodeConfig);
			Debug.Log(transportConfig);
			if (relayConfig.UseRelay)
				Debug.Log(relayConfig);

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
