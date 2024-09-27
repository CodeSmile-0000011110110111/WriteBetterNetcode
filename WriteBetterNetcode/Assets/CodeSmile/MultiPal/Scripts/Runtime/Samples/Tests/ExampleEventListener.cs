// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Samples.Tests
{
	[DisallowMultipleComponent]
	public sealed class ExampleEventListener : MonoBehaviour, INetworkEventListener
	{
		[SerializeField] private ExampleNetworkEvent m_ExampleNetworkEvent;

		public void OnNetworkEvent(NetworkEventData networkEventData) =>
			Debug.Log($"received network event: {networkEventData}");

		private void OnEnable()
		{
			if (m_ExampleNetworkEvent == null)
				throw new MissingReferenceException(nameof(m_ExampleNetworkEvent));

			//m_ExampleNetworkEvent = ScriptableObject.CreateInstance<ExampleNetworkEvent>();
			m_ExampleNetworkEvent.Register(this);
		}

		private void OnDisable() => m_ExampleNetworkEvent.Unregister(this);
	}
}
