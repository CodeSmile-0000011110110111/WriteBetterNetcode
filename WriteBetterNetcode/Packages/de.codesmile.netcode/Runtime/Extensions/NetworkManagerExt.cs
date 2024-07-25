// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;
using Object = System.Object;
#if UNITY_EDITOR
#endif

namespace CodeSmile.Netcode.Extensions
{
	/// <summary>
	///     Extension and utility methods for NetworkManager
	/// </summary>
	public static class NetworkManagerExt
	{
		private const String SingletonReadyName = "OnSingletonReady";
		private static HashSet<Action> OnSingletonReadyCallbacks;

		/// <summary>
		///     Shorthand for calling: NetworkManager.Singleton.GetComponent<UnityTransport>()
		/// </summary>
		/// <returns></returns>
		/// <returns>The UnityTransport component.</returns>
		public static UnityTransport GetTransport(this NetworkManager netMan) => netMan.GetComponent<UnityTransport>();

		/// <summary>
		///     Subscribe to NetworkManager's OnSingletonReady event, which is internal as of v1.8 (still internal in v2.0).
		///     Will continue to work even if OnSingletonReady may become public in the future.
		///     Will break if Unity were to rename the OnSingletonReady event.
		/// </summary>
		/// <example>
		///     Usage: use this in either the Awake or OnEnable method!
		///     Because in Start and later, NetworkManager.Singleton can no longer be null - unless there is no NetworkManager.
		/// </example>
		/// <remarks>
		///     By using this event handler you do not need to put your NetworkManager initialization dependent scripts
		///     in the Script Execution Order.
		/// </remarks>
		/// <remarks>
		///     The callback action will be invoked directly in case NetworkManager.Singleton is already non-null.
		/// </remarks>
		/// <remarks>
		///     Issue request to make OnSingletonReady public:
		///     https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/2386
		/// </remarks>
		/// <param name="callback">The action to be invoked when NetworkManager.Singleton has been assigned.</param>
		public static void InvokeWhenSingletonReady(Action callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			// singleton already initialized? => direct Invoke
			if (NetworkManager.Singleton != null)
			{
				callback.Invoke();
				return;
			}

			// one-time init of list and singleton ready event
			if (OnSingletonReadyCallbacks == null)
			{
				OnSingletonReadyCallbacks = new HashSet<Action>();
				Subscribe(GetSingletonReadyEvent(), InvokeSingletonReadyCallbacksOnce);
			}

			// put it in the queue to be called later
			OnSingletonReadyCallbacks.Add(callback);
		}

		private static void InvokeSingletonReadyCallbacksOnce()
		{
			Unsubscribe(GetSingletonReadyEvent(), InvokeSingletonReadyCallbacksOnce);

			foreach (var callback in OnSingletonReadyCallbacks)
				callback.Invoke();

			OnSingletonReadyCallbacks = null;
		}

		private static void Subscribe(EventInfo readyEvent, Action eventHandler) =>
			readyEvent.GetAddMethod(true).Invoke(readyEvent, new Object[] { eventHandler });

		private static void Unsubscribe(EventInfo readyEvent, Action eventHandler) =>
			readyEvent.GetRemoveMethod(true).Invoke(readyEvent, new Object[] { eventHandler });

		private static EventInfo GetSingletonReadyEvent()
		{
			var bindings = BindingFlags.Static | BindingFlags.NonPublic;
			var readyEvent = typeof(NetworkManager).GetEvent(SingletonReadyName, bindings);

			// if null, try to get the public version since it may be made public in the future (internal in Netcode 1.8.1)
			if (readyEvent == null)
			{
				bindings = BindingFlags.Static | BindingFlags.Public;
				readyEvent = typeof(NetworkManager).GetEvent(SingletonReadyName, bindings);
			}

			if (readyEvent == null)
				throw new MissingMemberException($"NetworkManager is missing the '{SingletonReadyName}' event.");

			return readyEvent;
		}

#if UNITY_EDITOR
		[InitializeOnLoadMethod] private static void ResetStaticFields() =>
			EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;

		private static void OnPlaymodeStateChanged(PlayModeStateChange playModeState)
		{
			if (playModeState == PlayModeStateChange.ExitingPlayMode)
				OnSingletonReadyCallbacks = null;
		}
#endif
	}
}
