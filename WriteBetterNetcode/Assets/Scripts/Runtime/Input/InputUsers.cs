// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using CodeSmile.Settings;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace CodeSmile.Input
{
	[DisallowMultipleComponent]
	public sealed class InputUsers : MonoBehaviour, GeneratedInputActions.IPairingActions
	{
		public event Action<InputUser, InputDevice> OnDevicePaired;
		public event Action<InputUser, InputDevice> OnDeviceUnpaired;

		public Boolean PairingEnabled
		{
			get => m_Actions[0].Pairing.enabled;
			set
			{
				foreach (var actions in m_Actions)
				{
					if (value)
						actions.Pairing.Enable();
					else
						actions.Pairing.Disable();
				}
			}
		}

		private InputUser HostUser { get => m_Users[0]; set => m_Users[0] = value; }

		private readonly InputUser[] m_Users =
			new InputUser[Constants.MaxCouchPlayers];

		private readonly GeneratedInputActions[] m_Actions =
			new GeneratedInputActions[Constants.MaxCouchPlayers];

		private void Awake()
		{
			CreateInputUsers();
			CreateInputActions();
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
		}

		private void CreateInputActions()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				m_Actions[playerIndex] = new GeneratedInputActions();
				m_Actions[playerIndex].Pairing.SetCallbacks(this);
			}
		}

		public void OnJoin(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Performed)
				TryPairUserDevice(context.control.device);
		}

		private void TryPairUserDevice(InputDevice device)
		{
			// only pair with gamepads; keyboard/mouse always bound to host
			if (device is not Gamepad)
				return;

			var deviceUser = InputUser.FindUserPairedToDevice(device);

			// if device is paired with host: unpair it first
			if (deviceUser != null && deviceUser.Value == HostUser)
			{
				HostUser.UnpairDevice(device);
				HostUser.AssociateActionsWithUser(m_Actions[0]);
				deviceUser = null;
			}

			// device without user => start pairing
			if (deviceUser == null)
			{
				// userIndex is -1 if all users already paired with a device
				var userIndex = GetUnpairedUserIndex();
				if (userIndex >= 0)
				{
					// pair and assign user's copy of actions (key layout!)
					var user = m_Users[userIndex];
					user = InputUser.PerformPairingWithDevice(device, user);
					user.AssociateActionsWithUser(m_Actions[userIndex]);
					m_Users[userIndex] = user; // write-back struct

					OnDevicePaired?.Invoke(user, device);
				}
			}
		}

		public void OnLeave(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Performed)
				TryUnpairUserDevice(context.control.device);
		}

		private void TryUnpairUserDevice(InputDevice device)
		{
			// only gamepads can be unpaired
			if (device is not Gamepad)
				return;

			// only already-paired, non-host players can unpair devices
			var deviceUser = InputUser.FindUserPairedToDevice(device);
			if (deviceUser != null && deviceUser != HostUser)
			{
				// unpair the device
				deviceUser.Value.UnpairDevice(device);
				deviceUser.Value.AssociateActionsWithUser(null);

				// "re-pair" device with host
				HostUser = InputUser.PerformPairingWithDevice(device, HostUser);

				OnDeviceUnpaired?.Invoke(deviceUser.Value, device);
			}
		}

		/// <summary>
		///     Find the index of the first user without a paired device.
		/// </summary>
		/// <returns>The index of an unpaired user or -1 if all users have a paired device.</returns>
		private Int32 GetUnpairedUserIndex()
		{
			for (var userIndex = 0; userIndex < m_Users.Length; userIndex++)
			{
				var user = m_Users[userIndex];
				if (user.pairedDevices.Count == 0 && user.lostDevices.Count == 0)
					return userIndex;
			}

			return -1;
		}
	}
}
