// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	[CreateAssetMenu(fileName = nameof(GameStates), menuName = "CodeSmile/GameStates/" + nameof(GameStates), order = 0)]
	public sealed class GameStates : ScriptableObject
	{
		[SerializeField] private GameStateAsset[] m_States = new GameStateAsset[0];
		[SerializeField] private Int32 m_PregameMenuStateIndex = 2;
		[SerializeField] private Int32 m_OfflineSingleplayerStateIndex = 4;
		public GameStateAsset this[Int32 stateIndex]
		{
			get
			{
				if (stateIndex < 0 || stateIndex >= m_States.Length)
					throw new IndexOutOfRangeException($"stateIndex {stateIndex} out of range (has {m_States.Length} states)");

				return m_States[stateIndex];
			}
		}
		public Int32 PregameMenuStateIndex => m_PregameMenuStateIndex;
		public Int32 OfflineSingleplayerStateIndex => m_OfflineSingleplayerStateIndex;

		private void OnEnable()
		{
			if (m_States.Length == 0)
				throw new ArgumentException("no game states assigned!");
		}

		public Int32 GetStateIndex(GameStateAsset state) => Array.IndexOf(m_States, state);
	}
}
