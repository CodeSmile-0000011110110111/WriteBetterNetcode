// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
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
	public sealed class InputUserState : MonoBehaviour, GeneratedInputActions.ISessionActions
	{
		public event Action<InputUser, InputDevice> OnDevicePaired;
		public event Action<InputUser, InputDevice> OnDeviceUnpaired;

		//[SerializeField] private InputActionAsset m_Actions;

		private readonly InputUser[] m_Users = new InputUser[Constants.MaxCouchPlayers];
		//private readonly InputActionMap[] m_ActiveActionMaps = new InputActionMap[Constants.MaxCouchPlayers];
		//private readonly Action<InputAction.CallbackContext>[] m_ActionTriggeredDelegates = new Action<InputAction.CallbackContext>[Constants.MaxCouchPlayers];
		//private Object m_OnJoinSessionActions;

		private Boolean m_UserDevicePairingEnabled = true;

		private GeneratedInputActions m_InputActions;

		private InputUser HostUser { get => m_Users[0]; set => m_Users[0] = value; }
		public Boolean UserDevicePairingEnabled
		{
			get => m_UserDevicePairingEnabled;
			set
			{
				m_UserDevicePairingEnabled = value;
				SetPairingActionsEnabled(m_UserDevicePairingEnabled);
			}
		}

		public void OnJoin(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Performed)
				TryPairUserDevice(context.control.device);
		}

		public void OnLeave(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Performed)
				TryUnpairUserDevice(context.control.device);
		}

		private void Awake()
		{
			//if (m_Actions == null) throw new MissingReferenceException("Input Actions not assigned");

			CreateInputUsers();

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

		private void OnEnable()
		{
			m_InputActions = new GeneratedInputActions();
			m_InputActions.Session.SetCallbacks(this);
			SetPairingActionsEnabled(m_UserDevicePairingEnabled);
		}

		private void OnDisable() => SetPairingActionsEnabled(false);

		private void Start() {}

		private void TryPairUserDevice(InputDevice device)
		{
			// only binding to gamepads supported, keyboard/mouse always bound to host player
			if (device is not Gamepad)
				return;

			var deviceUser = InputUser.FindUserPairedToDevice(device);
			deviceUser = TryUnpairDeviceFromHostUser(device, deviceUser);

			// device without user => start pairing
			if (deviceUser == null)
			{
				var userIndex = GetFirstUnpairedUserIndex();

				// we may have all users already paired ..
				if (userIndex >= 0)
				{
					var user = m_Users[userIndex];
					user = InputUser.PerformPairingWithDevice(device, user);
					m_Users[userIndex] = user;

					OnDevicePaired?.Invoke(user, device);

					//Debug.Log($"paired to {m_Users[userIndex]}: {device.name}");
				}
			}
		}

		private void TryUnpairUserDevice(InputDevice device)
		{
			// only gamepads can be unpaired
			if (device is not Gamepad)
				return;

			var deviceUser = InputUser.FindUserPairedToDevice(device);
			if (deviceUser != null)
			{
				// host cannot unpair devices
				if (deviceUser == HostUser)
					return;

				// unpair and pair it with host
				deviceUser.Value.UnpairDevice(device);
				HostUser = InputUser.PerformPairingWithDevice(device, HostUser);

				OnDeviceUnpaired?.Invoke(deviceUser.Value, device);

				//Debug.Log($"unpaired from {deviceUser}: {device.name}");
			}
		}

		private Int32 GetFirstUnpairedUserIndex()
		{
			for (var userIndex = 0; userIndex < m_Users.Length; userIndex++)
			{
				var user = m_Users[userIndex];
				if (user.pairedDevices.Count == 0)
					return userIndex;
			}

			return -1;
		}

		private InputUser? TryUnpairDeviceFromHostUser(InputDevice device, InputUser? pairedUser)
		{
			if (pairedUser != null && pairedUser.Value == HostUser)
			{
				pairedUser.Value.UnpairDevice(device);
				pairedUser = null;
			}

			return pairedUser;
		}

		private void SetPairingActionsEnabled(Boolean enabled)
		{
			if (enabled)
				m_InputActions.Session.Enable();
			else
				m_InputActions.Session.Disable();
		}

		private void CreateInputUsers()
		{
			// create users up-front to ensure indexes range from 0-3
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
				m_Users[playerIndex] = InputUser.CreateUserWithoutPairedDevices();

			// pair all unpaired devices with host user
			var unpairedDevices = InputUser.GetUnpairedInputDevices();
			foreach (var device in unpairedDevices)
				HostUser = InputUser.PerformPairingWithDevice(device, HostUser);

			Debug.Assert(HostUser.index == 0);
		}

		private void AssignActionTriggeredDelegates()
		{
			// m_ActionTriggeredDelegates[0] = OnActionTriggered_User0;
			// m_ActionTriggeredDelegates[1] = OnActionTriggered_User1;
			// m_ActionTriggeredDelegates[2] = OnActionTriggered_User2;
			// m_ActionTriggeredDelegates[3] = OnActionTriggered_User3;
		}

		private void SwitchActionMap(InputUser user, InputActionMap actionMap)
		{
			// var currentActionMap = m_ActiveActionMaps[user.index];
			// if (currentActionMap != null)
			// 	currentActionMap.actionTriggered -= m_ActionTriggeredDelegates[user.index];
			//
			// actionMap.actionTriggered += m_ActionTriggeredDelegates[user.index];
			// m_ActiveActionMaps[user.index] = actionMap;
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
			switch (change)
			{
				case InputDeviceChange.Added:
				case InputDeviceChange.Reconnected:
				case InputDeviceChange.Enabled:
					Debug.Log($"DEVICE {change}: {device}");
					OnInputDeviceAdded(device);
					break;
				case InputDeviceChange.Removed:
				case InputDeviceChange.Disconnected:
				case InputDeviceChange.Disabled:
					Debug.Log($"DEVICE {change}: {device}");
					OnInputDeviceRemoved(device);
					break;
				case InputDeviceChange.UsageChanged:
					Debug.Log($"DEVICE {change}: {device}");
					break;
				case InputDeviceChange.ConfigurationChanged:
					Debug.Log($"DEVICE {change}: {device}");
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
