// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Samples.Tests
{
	//[CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
	[CreateAssetMenu]
	public class GameEvent : ScriptableObject
	{
		private readonly List<GameEventListener> m_EventListeners = new();

		public void Invoke()
		{
			for (var i = m_EventListeners.Count - 1; i >= 0; i--)
				m_EventListeners[i].OnGameEvent();
		}

		public void AddListener(GameEventListener listener) => m_EventListeners.Add(listener);
		public void RemoveListener(GameEventListener listener) => m_EventListeners.Remove(listener);
	}
}
