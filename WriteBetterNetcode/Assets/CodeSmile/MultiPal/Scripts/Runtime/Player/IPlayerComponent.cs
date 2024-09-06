﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Player
{
	public interface IPlayerComponent
	{
		void OnPlayerSpawn(Int32 playerIndex);
		void OnPlayerDespawn(Int32 playerIndex);
	}
}
