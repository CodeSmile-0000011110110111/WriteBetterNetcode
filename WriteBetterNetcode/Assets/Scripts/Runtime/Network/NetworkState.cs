// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Extensions.Netcode;
using CodeSmile.Statemachine;
using CodeSmile.Statemachine.Netcode;
using CodeSmile.Statemachine.Netcode.Actions;
using System;
using System.Collections;
using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.BetterNetcode.Network
{
	public enum NetworkRole
	{
		Client,
		Host,
		Server,
	}

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
	public struct RelayConfig
	{
		public Boolean UseRelayService;
		public String RelayJoinCode;
	}

	public class NetworkState : MonoBehaviour
	{
		/// <summary>
		///     Raised whenever the network state changes.
		/// </summary>
		public event Action<StateChangedEventArgs> OnStateChanged;

		public enum State
		{
			Initializing,
			Offline,

			Starting,
			Online,
			ShuttingDown,

			ServerStarting,
			ServerOnline,
			ServerShuttingDown,

			HostStarting,
			HostOnline,
			HostShuttingDown,

			ClientConnecting,
			ClientConnected,
			ClientDisconnecting,
		}

		[SerializeField] private NetworkConfig m_ServerConfig = new() { Role = NetworkRole.Server };
		[SerializeField] private NetworkConfig m_HostConfig = new() { Role = NetworkRole.Host };
		[SerializeField] private NetworkConfig m_ClientConfig = new() { Role = NetworkRole.Client };

		public FSM m_Statemachine = new(nameof(NetworkState));
		private FSM.Variable m_StartServerVar;

		public State CurrentState { get; private set; } = State.Initializing;

		private void Awake() => SetupStatemachine();

		private void Start()
		{
			var net = NetworkManager.Singleton;
			net.OnClientStarted += OnClientStarted;
			net.OnClientStopped += OnClientStopped;
			net.OnServerStarted += OnServerStarted;
			net.OnServerStopped += OnServerStopped;
			net.OnTransportFailure += OnTransportFailure;

			// OnStateChanged += OnNetworkStateChanged;
			// ChangeState(State.Offline);

			m_Statemachine.Start().Evaluate();
			StartServer();
		}

		private void Update() => m_Statemachine.Evaluate();

		private void SetupStatemachine()
		{
			m_Statemachine.OnStateChange += args =>
				Debug.LogWarning($"{m_Statemachine} change: {args.PreviousState} to {args.ActiveState}");

			m_Statemachine.AllowMultipleStateChanges = true;
			m_StartServerVar = m_Statemachine.Vars.DefineBool("StartServerRequest");

			var states = FSM.S(Enum.GetNames(typeof(State)));
			m_Statemachine.WithStates(states);

			var initState = states[(Int32)State.Initializing];
			var offlineState = states[(Int32)State.Offline];
			var serverStartingState = states[(Int32)State.ServerStarting];
			var serverOnlineState = states[(Int32)State.ServerOnline];
			var serverShuttingDownState = states[(Int32)State.ServerShuttingDown];

			initState.WithTransitions(new FSM.Transition("Goto Offline", offlineState)
				.WithConditions(new IsNetworkManagerReady()));

			offlineState.WithTransitions(new FSM.Transition("StartServer", serverStartingState)
				.WithConditions(FSM.Variable.IsTrue(m_StartServerVar))
				.WithActions(new NetworkManagerStartServer()));

			serverStartingState.WithTransitions(new FSM.Transition("Server started", serverOnlineState)
				.WithConditions(new IsServerOnline()));
			serverOnlineState.WithTransitions(new FSM.Transition("Server stopped", serverShuttingDownState)
				.WithConditions(FSM.NOT(new IsServerOnline()))
				.WithActions(new NetworkManagerShutdown()));
			serverShuttingDownState.WithTransitions(new FSM.Transition("NetworkManager shutdown complete", offlineState)
				.WithConditions(new IsNetworkManagerReady()));
		}

		public void StartServer() => m_StartServerVar.BoolValue = true;

		private Boolean NetworkConfigFromMppmTags(out NetworkConfig config)
		{
			var role = NetworkRole.Client;
			var foundRole = false;

#if UNITY_EDITOR
			var tags = CurrentPlayer.ReadOnlyTags();
			for (var r = 0; r < 3; r++)
			{
				role = (NetworkRole)r;
				if (tags.Contains(((NetworkRole)r).ToString()))
				{
					foundRole = true;
					break;
				}
			}
#endif

			config = role == NetworkRole.Server ? m_ServerConfig : role == NetworkRole.Host ? m_HostConfig : m_ClientConfig;
			return foundRole;
		}

		private void OnNetworkStateChanged(StateChangedEventArgs args)
		{
			//Debug.Log($"NetworkState changed from {args.previousState} to {args.newState}");

			// switch (args.newState)
			// {
			// 	case State.Offline:
			// 		if (NetworkConfigFromMppmTags(out var config))
			// 			StartNetworking(config);
			// 		break;
			//
			// 	case State.Online:
			// 		//StopNetworking();
			// 		break;
			// }
		}

		private void CheckNetworkStateIs(State expectedState)
		{
			if (CurrentState != expectedState)
				throw new Exception($"State mismatch! Expected state: {expectedState} - Current state: {CurrentState}");
		}

		public void StartNetworking(NetworkConfig config)
		{
			Debug.Log($"StartNetworking({config})");
			CheckNetworkStateIs(State.Offline);
			ChangeState(State.Starting);

			var net = NetworkManager.Singleton;
			var transport = net.GetTransport();
			transport.ConnectionData = config.AddressData;
			transport.UseWebSockets = config.UseWebSockets;
			transport.UseEncryption = config.UseEncryption;

			switch (config.Role)
			{
				case NetworkRole.Client:
					if (net.StartClient() == false)
					{
						Debug.LogWarning("StartClient failed");
						ChangeState(State.ShuttingDown);
					}
					break;
				case NetworkRole.Host:
					if (net.StartHost() == false)
					{
						Debug.LogWarning("StartHost failed");
						ChangeState(State.ShuttingDown);
					}
					break;
				case NetworkRole.Server:
					if (net.StartServer() == false)
					{
						Debug.LogWarning("StartServer failed");
						ChangeState(State.ShuttingDown);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(config.Role), config.Role, null);
			}
		}

		private void StopNetworking()
		{
			CheckNetworkStateIs(State.Online);

			ChangeState(State.ShuttingDown);
			var net = NetworkManager.Singleton;
			net.Shutdown();
		}

		private void OnClientStarted() => ChangeState(State.Online);

		private void OnClientStopped(Boolean isHost)
		{
			// for hosts we'll wait for the OnServerStopped event
			if (isHost == false)
				GoOfflineAtEndOfFrame();
		}

		private void OnServerStarted() => ChangeState(State.Online);
		private void OnServerStopped(Boolean isHost) => GoOfflineAtEndOfFrame();
		private void OnTransportFailure() => ChangeState(State.ShuttingDown); // will internally call Shutdown()

		private void GoOfflineAtEndOfFrame()
		{
			// in case we ever call this twice in a row (ie for the host)
			StopAllCoroutines();

			// After NetworkManager.Shutdown() and subsequent OnServerStopped / OnClientStopped events
			// we MUST wait for the shutdown to complete before we can start a network session again!
			StartCoroutine(GoOfflineAtEndOfFrameCoroutine());
		}

		private IEnumerator GoOfflineAtEndOfFrameCoroutine()
		{
			yield return new WaitUntil(() =>
			{
				var net = NetworkManager.Singleton;
				return net == null ||
				       (net.ShutdownInProgress || net.IsListening || net.IsServer || net.IsHost || net.IsClient) == false;
			});

			// then also wait until after Update, LateUpdate, Rendering of current frame
			yield return new WaitForEndOfFrame();

			ChangeState(State.Offline);
		}

		private void ChangeState(State newState)
		{
			if (CurrentState != newState)
			{
				var stateChangeEventArgs = new StateChangedEventArgs { previousState = CurrentState, newState = newState };

				CurrentState = newState;
				OnStateChanged?.Invoke(stateChangeEventArgs);
			}
		}

		public struct StateChangedEventArgs
		{
			/// <summary>
			///     Previous state
			/// </summary>
			public State previousState;
			/// <summary>
			///     Current state
			/// </summary>
			public State newState;
		}
	}
}
