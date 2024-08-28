﻿// Copyright (C) 2021-2024 Steffen Itterheim
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
	public sealed class InputUsers : MonoBehaviour,
		GeneratedInputActions.IPairingActions
	{
		public event Action<InputUser, InputDevice> OnDevicePaired;
		public event Action<InputUser, InputDevice> OnDeviceUnpaired;

		private readonly InputUser[] m_Users = new InputUser[Constants.MaxCouchPlayers];
		private readonly GeneratedInputActions[] m_Actions = new GeneratedInputActions[Constants.MaxCouchPlayers];
		public GeneratedInputActions[] Actions => m_Actions;

		public Boolean PairingEnabled
		{
			get => Actions[0].Pairing.enabled;
			set
			{
				foreach (var actions in Actions)
				{
					if (value)
					{
						actions.Pairing.Enable();
						actions.Pairing.Join.Enable();
						actions.Pairing.Leave.Disable();
					}
					else
					{
						actions.Pairing.Disable();
						actions.Pairing.Join.Disable();
						actions.Pairing.Leave.Disable();
					}
				}
			}
		}

		private InputUser HostUser { get => m_Users[0]; set => m_Users[0] = value; }

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
			PairUnpairedDevicesWithHostUser();
			CreateInputActions();

			InputSystem.onDeviceChange += OnDeviceChange;
		}

		private void CreateInputUsers()
		{
			// create users up-front to ensure indexes range from 0-3
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
				m_Users[playerIndex] = InputUser.CreateUserWithoutPairedDevices();
		}

		private void PairUnpairedDevicesWithHostUser()
		{
			if (HostUser.valid == false)
				return;

			var unpairedDevices = InputUser.GetUnpairedInputDevices();
			foreach (var device in unpairedDevices)
				HostUser = InputUser.PerformPairingWithDevice(device, HostUser);
		}

		private void CreateInputActions()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				Actions[playerIndex] = new GeneratedInputActions();
				Actions[playerIndex].Pairing.SetCallbacks(this);
			}
		}

		private void OnDeviceChange(InputDevice device, InputDeviceChange change)
		{
			if (change == InputDeviceChange.Added)
				PairUnpairedDevicesWithHostUser();
		}

		private void TryPairUserDevice(InputDevice device)
		{
			// only pair with gamepads; keyboard/mouse always bound to host
			if (device is not Gamepad)
				return;

			var deviceUser = InputUser.FindUserPairedToDevice(device);
			if (deviceUser != null && deviceUser.Value == HostUser)
			{
				// if device is paired with host: unpair
				HostUser.UnpairDevice(device);
				HostUser.AssociateActionsWithUser(Actions[0]);
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
					user.AssociateActionsWithUser(Actions[userIndex]);
					m_Users[userIndex] = user; // write-back struct

					EnableLeaveAction(userIndex);

					OnDevicePaired?.Invoke(user, device);
				}
			}
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

				EnableJoinAction(deviceUser.Value.index);

				OnDeviceUnpaired?.Invoke(deviceUser.Value, device);
			}
		}

		public void UnpairAll()
		{
			foreach (var user in m_Users)
			{
				foreach (var device in user.pairedDevices)
					TryUnpairUserDevice(device);
				foreach (var device in user.lostDevices)
					TryUnpairUserDevice(device);
			}
		}

		private void EnableLeaveAction(Int32 userIndex)
		{
			m_Actions[userIndex].Pairing.Join.Disable();
			m_Actions[userIndex].Pairing.Leave.Enable();
		}

		private void EnableJoinAction(Int32 userIndex)
		{
			m_Actions[userIndex].Pairing.Join.Enable();
			m_Actions[userIndex].Pairing.Leave.Disable();
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