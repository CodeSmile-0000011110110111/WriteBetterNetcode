// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Netcode.Extensions;
using System;
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
			NotReady,
			Offline,

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

		private State m_State = State.NotReady;

		private void Awake()
		{
			Debug.Log("Awake NetworkState");
			NetworkManagerExt.InvokeWhenSingletonReady(OnNetworkManagerSingletonReady);

			OnStateChanged += args =>
			{
				Debug.Log($"state changed from {args.previousState} to {args.newState}");

				if (args.newState == State.Offline)
				{
					StartServer();
				}
			};
		}

		private void OnNetworkManagerSingletonReady()
		{
			if (m_State == State.NotReady)
				ChangeState(State.Offline);
		}

		public void StartServer()
		{
			if (m_State != State.Offline)
				throw new Exception($"invalid state {m_State}, can't start server");

			Debug.Log("NetworkState.StartServer()");
			NetworkManager.Singleton.StartServer();
		}

		public void StartHost()
		{

		}

		public void StartClient()
		{

		}

		private void ChangeState(State newState)
		{
			if (m_State != newState)
			{
				var stateChangeEventArgs = new StateChangedEventArgs { previousState = m_State, newState = newState };

				m_State = newState;
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

		//private void Update() => Debug.Log($"state: {m_State}");
	}
}
