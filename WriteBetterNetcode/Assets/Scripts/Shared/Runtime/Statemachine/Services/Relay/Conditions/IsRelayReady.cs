﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Relay.Conditions
{
	public class IsRelayReady : ICondition
	{
		private readonly Var<RelayConfig> m_RelayConfigVar;
		private IsRelayReady() {}

		public IsRelayReady(Var<RelayConfig> relayConfigVar) => m_RelayConfigVar = relayConfigVar;

		public Boolean IsSatisfied(FSM sm) => m_RelayConfigVar.Value.IsReady;
	}
}
