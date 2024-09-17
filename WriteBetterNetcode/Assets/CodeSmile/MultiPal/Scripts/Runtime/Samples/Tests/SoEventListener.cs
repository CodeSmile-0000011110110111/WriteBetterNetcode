// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace CodeSmile.MultiPal.Samples.Tests
{
	[DisallowMultipleComponent]
	public sealed class NetworkEventListener : MonoBehaviour, INetworkEventListener
	{
		[SerializeField] private NetworkEventBase m_NetworkEvent;
		[SerializeField] private UnityEvent m_UnityEvent;

		private void OnEnable() => m_NetworkEvent.Register(this);
		private void OnDisable() => m_NetworkEvent.Unregister(this);
		public void OnNetworkEvent() => m_UnityEvent.Invoke();
		public void OnNetworkEvent(NetworkEventData networkEventData)
		{
			throw new System.NotImplementedException();
		}
	}
}
