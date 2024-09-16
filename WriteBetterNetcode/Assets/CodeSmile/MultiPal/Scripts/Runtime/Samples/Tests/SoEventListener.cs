// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace CodeSmile.MultiPal.Samples.Tests
{
	public class GameEventListener : MonoBehaviour
	{
		[SerializeField] private GameEvent m_GameEvent;
		[SerializeField] private UnityEvent m_UnityEvent;

		private void OnEnable() => m_GameEvent.AddListener(this);
		private void OnDisable() => m_GameEvent.RemoveListener(this);
		public void OnGameEvent() => m_UnityEvent.Invoke();
	}
}
