// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Player
{
	[DisallowMultipleComponent]
	public sealed class PlayerController : MonoBehaviour, IPlayerComponent //, IAnimatorParametersProvider
	{
		private Int32 m_PlayerIndex;

		//private PlayerControllers m_PlayerControllers;
		// private PlayerControllerBase ActiveController => m_PlayerControllers?.GetActiveController(m_PlayerIndex);
		// public AnimatorParametersBase AnimatorParameters
		// {
		// 	get => ActiveController?.AnimatorParameters;
		// 	set
		// 	{
		// 		var ctrl = ActiveController;
		// 		if (ctrl != null)
		// 			ctrl.AnimatorParameters = value;
		// 	}
		// }

		public void OnPlayerSpawn(Int32 playerIndex) => m_PlayerIndex = playerIndex;

		// m_PlayerControllers = ComponentsRegistry.Get<PlayerControllers>();
		public void OnPlayerDespawn(Int32 playerIndex) {}
	}
}
