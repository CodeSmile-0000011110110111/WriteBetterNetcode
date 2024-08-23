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

		private readonly InputUser[] m_Users = new InputUser[Constants.MaxCouchPlayers];
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

		private void OnActionTriggered(Int32 playerIndex, InputAction.CallbackContext ctx) =>
			Debug.Log($"User #{playerIndex}: {ctx.action.name} {ctx.phase}");

		private void OnInputActionChange(Object action, InputActionChange change) => Debug.Log($"{change}: {action}");

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
					break;
				case InputDeviceChange.Removed:
				case InputDeviceChange.Disconnected:
				case InputDeviceChange.Disabled:
					Debug.Log($"DEVICE {change}: {device}");
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
	}
}
