// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.BetterNetcode.Network
{
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
		}

		public enum Role
		{
			Client,
			Host,
			Server,
		}

		public State CurrentState { get; private set; } = State.Initializing;
		public Role CurrentRole { get; private set; } = Role.Client;

		private Boolean IsMppmServer
		{
			get
			{
#if UNITY_EDITOR
				return CurrentPlayer.ReadOnlyTags().Contains(Role.Server.ToString());
#else
				return false;
#endif
			}
		}

		private void Start()
		{
			var net = NetworkManager.Singleton;
			net.OnClientStarted += OnClientStarted;
			net.OnClientStopped += OnClientStopped;
			net.OnServerStarted += OnServerStarted;
			net.OnServerStopped += OnServerStopped;

			OnStateChanged += OnNetworkStateChanged;

			ChangeState(State.Offline);
		}

		private Boolean RoleFromMppmTags(out Role role)
		{
			role = Role.Client;
			var foundRole = false;

#if UNITY_EDITOR
			var tags = CurrentPlayer.ReadOnlyTags();
			for (var r = 0; r < 3; r++)
			{
				role = (Role)r;
				if (tags.Contains(((Role)r).ToString()))
				{
					foundRole = true;
					break;
				}
			}
#endif

			return foundRole;
		}

		private void OnNetworkStateChanged(StateChangedEventArgs args)
		{
			Debug.Log($"NetworkState changed from {args.previousState} to {args.newState}");

			switch (args.newState)
			{
				case State.Offline:
					if (RoleFromMppmTags(out var role))
						StartNetworking(role);
					break;

				case State.Online:
					StopNetworking();
					break;
			}
		}

		private void CheckNetworkStateIs(State expectedState)
		{
			if (CurrentState != expectedState)
				throw new Exception($"State mismatch! Expected state: {expectedState} - Current state: {CurrentState}");
		}

		public void StartNetworking(Role role)
		{
			CheckNetworkStateIs(State.Offline);

			Debug.Log($"Start networking with role: {role}");
			CurrentRole = role;
			ChangeState(State.Starting);

			var net = NetworkManager.Singleton;
			switch (role)
			{
				case Role.Client:
					if (net.StartClient())
						ChangeState(State.ShuttingDown);
					break;
				case Role.Host:
					if (net.StartHost())
						ChangeState(State.ShuttingDown);
					break;
				case Role.Server:
					if (net.StartServer())
						ChangeState(State.ShuttingDown);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(role), role, null);
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
			if (isHost == false)
				GoOfflineAtEndOfFrame();
		}

		private void OnServerStarted() => ChangeState(State.Online);
		private void OnServerStopped(Boolean isHost) => GoOfflineAtEndOfFrame();

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
