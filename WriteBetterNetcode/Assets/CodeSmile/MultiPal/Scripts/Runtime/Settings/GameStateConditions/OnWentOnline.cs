// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.Statemachine.Netcode;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings.GameStateConditions
{
	[CreateAssetMenu(fileName = nameof(OnWentOnline), menuName = "CodeSmile/" + nameof(OnWentOnline),
		order = 0)]
	public sealed class OnWentOnline : GameStateConditionBase
	{
		private Boolean m_IsOnline;

		public override void OnEnterState()
		{
			m_IsOnline = false;
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			netcodeState.WentOnline += WentOnline;
		}

		public override void OnExitState()
		{
			var netcodeState = ComponentsRegistry.Get<NetcodeState>();
			netcodeState.WentOnline -= WentOnline;
		}

		private void WentOnline(NetcodeRole role) => m_IsOnline = true;

		public override Boolean IsSatisfied() => m_IsOnline;
	}
}
