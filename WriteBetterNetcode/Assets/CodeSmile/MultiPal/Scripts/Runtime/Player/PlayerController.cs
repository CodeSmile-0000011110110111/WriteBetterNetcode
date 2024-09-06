// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using CodeSmile.Components.Utility;
using CodeSmile.MultiPal.Animation;
using CodeSmile.MultiPal.Input;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.MultiPal.Player
{
	// FIXME: entschlack this ... most of things can be moved or forwarded to PlayerControllers
	[DisallowMultipleComponent]
	public sealed class PlayerController : MonoBehaviour, IPlayerComponent, GeneratedInput.IPlayerKinematicsActions,
		IAnimatorParametersProvider
	{
		private Int32 m_PlayerIndex;

		private PlayerControllers m_PlayerControllers;
		private PlayerControllerBase ActiveController => m_PlayerControllers?.GetActiveController(m_PlayerIndex);
		public AnimatorParametersBase AnimatorParameters
		{
			get => ActiveController?.AnimatorParameters;
			set
			{
				if (ActiveController != null)
					ActiveController.AnimatorParameters = value;
			}
		}

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;
			m_PlayerControllers = ComponentsRegistry.Get<PlayerControllers>();

			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.SetPlayerKinematicsCallback(playerIndex, this);
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.SetPlayerKinematicsCallback(playerIndex, null);
		}

		public void OnMove(InputAction.CallbackContext context) => ActiveController.OnMove(context);
		public void OnLook(InputAction.CallbackContext context) => ActiveController.OnLook(context);
		public void OnCrouch(InputAction.CallbackContext context) => ActiveController.OnCrouch(context);
		public void OnJump(InputAction.CallbackContext context) => ActiveController.OnJump(context);
		public void OnSprint(InputAction.CallbackContext context) => ActiveController.OnSprint(context);

		public void PreviousController() => m_PlayerControllers.SetPreviousControllerActive(m_PlayerIndex);

		public void NextController() => m_PlayerControllers.SetNextControllerActive(m_PlayerIndex);
	}
}
