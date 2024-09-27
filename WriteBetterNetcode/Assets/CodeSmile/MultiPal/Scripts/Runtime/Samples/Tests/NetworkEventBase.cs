// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.MultiPal.Samples.Tests
{
	// FIXME: what's the benefit of having this as SO?
	// FIXME: make base generic type with generic params

	//[CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
	public abstract class NetworkEventBase : ScriptableObject
	{
		private readonly List<INetworkEventListener> m_Listeners = new();

		protected NetworkEventData NetworkEventData;

		protected void Invoke()
		{
			for (var i = m_Listeners.Count - 1; i >= 0; i--)
				m_Listeners[i].OnNetworkEvent(NetworkEventData);
		}

		public void Register(INetworkEventListener listener) => m_Listeners.Add(listener);
		public void Unregister(INetworkEventListener listener) => m_Listeners.Remove(listener);

		// send writer
		// receive reader

		// went online/offline => (un)register events, enable send/receive

		// invoke the custom event with custom parameters

		public void Send() => NetworkEventData.Send(new());

		public void Receive() => NetworkEventData.Receive(new());
	}

	public struct NetworkEventData
	{
		public Object Data;
		public static void Send(FastBufferWriter fastBufferWriter) => throw new NotImplementedException();
		public static void Receive(FastBufferReader fastBufferReader) => throw new NotImplementedException();
	}
}
