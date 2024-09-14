// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.Statemachine.Netcode;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings.GameStates
{
	[CreateAssetMenu(fileName = nameof(OnWentOffline), menuName = GameStateBase.MenuRoot + nameof(OnWentOffline),
		order = 0)]
	public sealed class OnWentOffline : GameStateConditionBase
	{
		private Boolean m_IsOffline;

		public override void OnEnterState()
		{
			m_IsOffline = false;
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			netcodeState.WentOffline += WentOffline;
		}

		public override void OnExitState()
		{
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			netcodeState.WentOffline -= WentOffline;
		}

		private void WentOffline(NetcodeRole role) => m_IsOffline = true;

		public override Boolean IsSatisfied() => m_IsOffline;
	}
}
