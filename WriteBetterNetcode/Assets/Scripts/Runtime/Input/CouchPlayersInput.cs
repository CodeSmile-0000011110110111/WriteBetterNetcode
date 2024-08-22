// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Settings;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Object = System.Object;

namespace CodeSmile
{
	[DisallowMultipleComponent]
	public sealed class CouchPlayersInput : MonoBehaviour
	{
		[SerializeField] private InputActionAsset m_Actions;

		private readonly InputUser[] m_Users = new InputUser[Constants.MaxCouchPlayers];
		private readonly InputActionMap[] m_ActiveActionMaps = new InputActionMap[Constants.MaxCouchPlayers];
		private readonly Action<InputAction.CallbackContext>[] m_ActionTriggeredDelegates =
			new Action<InputAction.CallbackContext>[Constants.MaxCouchPlayers];

		private InputUser HostUser => m_Users[0];

		private void Awake()
		{
			if (m_Actions == null)
				throw new MissingReferenceException("Input Actions not assigned");

			CreateInputUsers();
			AssignActionTriggeredDelegates();

			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				var user = m_Users[playerIndex];

				for (var i = 0; i < m_Actions.actionMaps.Count; i++)
				{
					var actionMap = m_Actions.actionMaps[i];
					Debug.Log($"action maps: {actionMap.name}");

					if (playerIndex == 0 && i == 0)
						SwitchActionMap(user, actionMap);
				}
			}

			// start with only first input active
			// SetAllPlayerInputsActive(false);
			// SetPlayerInputActive(0, true);

			InputSystem.onDeviceChange += OnInputDeviceChange;
			InputSystem.onActionChange += OnInputActionChange;
			InputUser.onChange += OnInputUserChange;
		}

		private void OnDestroy()
		{
			InputSystem.onDeviceChange -= OnInputDeviceChange;
			InputSystem.onActionChange -= OnInputActionChange;
			InputUser.onChange -= OnInputUserChange;
		}

		private void Start() {}

		private void CreateInputUsers()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				m_Users[playerIndex] = new InputUser();
				Debug.Assert(m_Users[playerIndex].index == playerIndex);
			}

			m_Users[0] = InputUser.PerformPairingWithDevice(Keyboard.current, HostUser);
			InputUser.PerformPairingWithDevice(Mouse.current, HostUser);
		}

		private void AssignActionTriggeredDelegates()
		{
			m_ActionTriggeredDelegates[0] = OnActionTriggered_User0;
			m_ActionTriggeredDelegates[1] = OnActionTriggered_User1;
			m_ActionTriggeredDelegates[2] = OnActionTriggered_User2;
			m_ActionTriggeredDelegates[3] = OnActionTriggered_User3;
		}

		private void SwitchActionMap(InputUser user, InputActionMap actionMap)
		{
			var currentActionMap = m_ActiveActionMaps[user.index];
			if (currentActionMap != null)
				currentActionMap.actionTriggered -= m_ActionTriggeredDelegates[user.index];

			actionMap.actionTriggered += m_ActionTriggeredDelegates[user.index];
			m_ActiveActionMaps[user.index] = actionMap;
		}

		private void OnActionTriggered_User0(InputAction.CallbackContext ctx) => OnActionTriggered(0, ctx);
		private void OnActionTriggered_User1(InputAction.CallbackContext ctx) => OnActionTriggered(1, ctx);
		private void OnActionTriggered_User2(InputAction.CallbackContext ctx) => OnActionTriggered(2, ctx);
		private void OnActionTriggered_User3(InputAction.CallbackContext ctx) => OnActionTriggered(3, ctx);

		private void OnActionTriggered(Int32 playerIndex, InputAction.CallbackContext ctx) =>
			Debug.Log($"User #{playerIndex}: {ctx.action.name} {ctx.phase}");

		private void OnInputActionChange(Object action, InputActionChange change)
		{
			//Debug.Log($"{change}: {action}");
		}

		private void OnInputUserChange(InputUser user, InputUserChange change, InputDevice device) =>
			Debug.Log($"USER {change}: {user} - {device}");

		private void OnInputDeviceChange(InputDevice device, InputDeviceChange change)
		{
			Debug.Log($"DEVICE {change}: {device}");

			switch (change)
			{
				case InputDeviceChange.Added:
					//case InputDeviceChange.Reconnected:
					//case InputDeviceChange.Enabled:
					OnInputDeviceAdded(device);
					break;
				case InputDeviceChange.Removed:
					//case InputDeviceChange.Disconnected:
					//case InputDeviceChange.Disabled:
					OnInputDeviceRemoved(device);
					break;
				case InputDeviceChange.UsageChanged:
					break;
				case InputDeviceChange.ConfigurationChanged:
					break;
				case InputDeviceChange.SoftReset:
					break;
				case InputDeviceChange.HardReset:
					break;
			}
		}

		private void OnInputDeviceAdded(InputDevice device)
		{
			// var playerIndex = GetInactivePlayerIndex();
			// if (playerIndex >= 0)
			// {
			// 	//SetPlayerInputActive(playerIndex, true);
			// 	LogInputDevicePairing();
			// }
		}

		private void OnInputDeviceRemoved(InputDevice device)
		{
			// var playerIndex = GetPlayerIndexForDevice(device);
			// if (playerIndex >= 0)
			// {
			// 	SetPlayerInputActive(playerIndex, false);
			// 	LogInputDevicePairing();
			// }
		}

		/*
		[SerializeField] private PlayerInput[] m_PlayersInput = new PlayerInput[Constants.MaxCouchPlayers];

		private void PairHostPlayerWithDevices()
		{
			// Host player pairs with all connected devices
			var didPairMouseKeyboard = false;
			var didPairGamepadJoystick = false;

			var devices = InputSystem.devices;
			for (var deviceIndex = 0; deviceIndex < devices.Count; deviceIndex++)
			{
				var device = devices[deviceIndex];
				if (device != null)
				{
					Debug.Log($"{device}");

					didPairMouseKeyboard = device is Mouse || device is Keyboard || didPairMouseKeyboard;
					didPairGamepadJoystick = device is Gamepad || device is Joystick || didPairGamepadJoystick;

					SetPlayerInputActive(0, true);

					if (didPairMouseKeyboard && didPairGamepadJoystick)
					{
						break;
					}
				}
			}
		}

		private Boolean IsKeyboardOrMouse(InputDevice device) =>
			device == Keyboard.current.device || device == Mouse.current.device;

		private void SetAllPlayerInputsActive(Boolean active)
		{
			foreach (var playerInput in m_PlayersInput)
				playerInput.gameObject.SetActive(active);
		}

		private void SetPlayerInputActive(Int32 playerIndex, Boolean active)
		{
			Debug.Log($"SetPlayerInputActive({playerIndex}, active={active})");
			m_PlayersInput[playerIndex].gameObject.SetActive(active);
		}

		private Int32 GetInactivePlayerIndex()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				var playerInput = m_PlayersInput[playerIndex];
				if (playerInput.gameObject.activeSelf == false)
					return playerIndex;
			}

			return -1;
		}

		private Int32 GetPlayerIndexForDevice(InputDevice device)
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				var playerInput = m_PlayersInput[playerIndex];
				if (playerInput.devices.Contains(device))
					return playerIndex;
			}

			return -1;
		}

		internal static void LogInputDevicePairing()
		{
			var devices = InputSystem.devices;
			for (var i = 0; i < devices.Count; i++)
			{
				var device = devices[i];
				if (device != null)
				{
					var pairedInput = PlayerInput.FindFirstPairedToDevice(device);
					if (pairedInput != null)
						Debug.Log($"{pairedInput.name} paired with {device.name}");
				}
			}
		}
		*/
	}
}
