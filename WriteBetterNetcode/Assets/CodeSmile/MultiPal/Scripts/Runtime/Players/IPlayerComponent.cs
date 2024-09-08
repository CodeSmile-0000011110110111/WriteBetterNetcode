// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players
{
	public interface IPlayerComponent
	{
		void OnPlayerSpawn(Int32 playerIndex, Boolean isOwner);
		void OnPlayerDespawn(Int32 playerIndex, Boolean isOwner);
	}
}
