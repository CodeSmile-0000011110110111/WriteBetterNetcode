﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	public class IsNotListening : ICondition
	{
		public Boolean IsSatisfied(FSM sm) => !NetworkManager.Singleton.IsListening;
	}
}
