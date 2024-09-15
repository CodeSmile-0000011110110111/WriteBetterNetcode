// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Design
{
	[DisallowMultipleComponent]
	public class SpawnLocation : MonoBehaviour
	{
		[Flags]
		public enum Players
		{
			Player1 = 1 << 0,
			Player2 = 1 << 1,
			Player3 = 1 << 2,
			Player4 = 1 << 3,
			AnyPlayer = Player1 | Player2 | Player3 | Player4,
		}

		[SerializeField] private Players m_PlayersAllowed = Players.AnyPlayer;

		public Players PlayersAllowed => m_PlayersAllowed;

		public Boolean IsPlayerAllowed(Int32 playerIndex) => (m_PlayersAllowed & (Players)(1 << playerIndex)) != 0;
	}
}
