// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

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
			SigningIn,

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
		private FSM.StructVar<NetcodeConfig> m_NetcodeConfigVar;
		private FSM.StructVar<RelayConfig> m_RelayConfigVar;
		private FSM.StructVar<TransportConfig> m_TransportConfigVar;

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

		private void Update() =>
			//Debug.Log($"[{Time.frameCount}] {m_Statemachine.Name} updating ...");
			m_Statemachine.Update();

		//Debug.Log($"[{Time.frameCount}] {m_Statemachine.Name} updating DONE ...");
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
			var signInState = states[(Int32)State.SigningIn];
			var serverStartingState = states[(Int32)State.ServerStarting];
			var serverStartedState = states[(Int32)State.ServerStarted];
			var clientStartingState = states[(Int32)State.ClientStarting];
			var clientStartedState = states[(Int32)State.ClientStarted];
			var clientConnectedState = states[(Int32)State.ClientConnected];
			var clientDisconnectedState = states[(Int32)State.ClientDisconnected];
			var stoppingState = states[(Int32)State.Stopping];

			// Init state
			initState.AddTransition("Init Complete")
				.ToState(offlineState)
				.WithConditions(new IsNetworkOffline())
				.WithActions(new UnityServicesInit());

			// Sign In
			// TODO: handle signIn throwing an exception!!
			offlineState.AddTransition("SignInAnonymously")
				.ToState(signInState)
				.WithConditions(
					new IsNotSignedIn(),
					new IsRelayEnabled(m_RelayConfigVar))
				.WithActions(new SignInAnonymously());
			signInState.AddTransition("SignedIn")
				.ToState(offlineState)
				.WithConditions(new IsSignedIn());

			// Offline state
			offlineState.AddTransition("Start Server")
				.ToState(serverStartingState)
				.WithConditions(
					new IsSignedIn(),
					new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.Server))
				.WithActions(
					new ApplyTransportConfig(m_TransportConfigVar),
					new NetworkStart(NetcodeRole.Server));
			offlineState.AddTransition("Start Host")
				.ToState(serverStartingState)
				.WithConditions(
					new IsSignedIn(),
					new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.Host))
				.WithActions(
					new ApplyTransportConfig(m_TransportConfigVar),
					new NetworkStart(NetcodeRole.Host));
			offlineState.AddTransition("Start Client")
				.ToState(clientStartingState)
				.WithConditions(
					new IsSignedIn(),
					new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.Client))
				.WithActions(
					new ApplyTransportConfig(m_TransportConfigVar),
					new NetworkStart(NetcodeRole.Client));

			// Server States
			serverStartingState.AddTransition("Server started")
				.ToState(serverStartedState)
				.WithConditions(new IsLocalServerStarted());
			serverStartedState.AddTransition("Server stopping")
				.ToState(stoppingState)
				.WithConditions(FSM.OR(
					FSM.NOT(new IsLocalServerStarted()),
					new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None)))
				.WithActions(new NetworkStop());

			// Client States
			clientStartingState.AddTransition("Client started")
				.ToState(clientStartedState)
				.WithConditions(new IsLocalClientStarted());
			clientStartedState.AddTransition("Client connected")
				.ToState(clientConnectedState)
				.WithConditions(new IsLocalClientConnected());
			clientConnectedState.AddTransition("Client disconnected")
				.ToState(clientDisconnectedState)
				.WithConditions(FSM.NOT(new IsLocalClientConnected()));

			// Stopping state
			stoppingState.AddTransition("Network stopping")
				.ToState(offlineState)
				.WithConditions(new IsNetworkOffline())
				.WithActions(new SetNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None));

			// stop transition gets added to multiple states
			FSM.CreateTransition("Client stopping")
				.ToState(stoppingState)
				.AddToStates(clientStartedState, clientConnectedState, clientDisconnectedState)
				.WithConditions(FSM.OR(
					FSM.NOT(new IsLocalClientStarted()),
					new IsNetcodeRole(m_NetcodeConfigVar, NetcodeRole.None)))
				.WithActions(new NetworkStop());
		}

		public void RequestStartServer()
		{
			m_NetcodeConfigVar.Value = new NetcodeConfig { Role = NetcodeRole.Server };
			m_RelayConfigVar.Value = new RelayConfig { UseRelayService = true };
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
