// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using CodeSmile.Settings;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace CodeSmile.Player
{
	// receive all input and forward to specific player instance by index
	public class CouchPlayersInput : MonoBehaviour, GeneratedInputActions.IPlayerActions, GeneratedInputActions.IUIActions
	{
		private readonly GeneratedInputActions.IPlayerActions[] m_Callbacks =
			new GeneratedInputActions.IPlayerActions[Constants.MaxCouchPlayers];

		public GeneratedInputActions.IPlayerActions this[Int32 playerIndex]
		{
			get => m_Callbacks[playerIndex];
			set => m_Callbacks[playerIndex] = value;
		}

		private static Boolean GetPlayerIndex(InputAction.CallbackContext context, out Int32 playerIndex)
		{
			var user = InputUser.FindUserPairedToDevice(context.control.device);
			playerIndex = user != null ? user.Value.index : -1;
			return playerIndex >= 0;
		}

		// Player
		public void OnMove(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnLook(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnAttack(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnInteract(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnCrouch(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnJump(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnPrevious(InputAction.CallbackContext context)
		{
			if (GetPlayerIndex(context, out var playerIndex))
			{
				Debug.Log($"User #{playerIndex}: {context.action.name}");
				m_Callbacks[playerIndex]?.OnPrevious(context);
			}
		}

		public void OnNext(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnSprint(InputAction.CallbackContext context) => throw new System.NotImplementedException();

		// UI
		public void OnNavigate(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnSubmit(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnCancel(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnPoint(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnClick(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnRightClick(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnMiddleClick(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnScrollWheel(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnTrackedDevicePosition(InputAction.CallbackContext context) => throw new System.NotImplementedException();
		public void OnTrackedDeviceOrientation(InputAction.CallbackContext context) => throw new System.NotImplementedException();
	}
}
