// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace CodeSmile
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerInput))]
	public class TestInput : MonoBehaviour
	{
		private PlayerInput m_Input;

		private Int32 m_FrameJoined;

		private void Start()
		{
			m_Input = GetComponent<PlayerInput>();
		}

		public void OnLook(InputValue dir)
		{
			//Debug.Log($"OnLook: {dir.Get<Vector2>()}");
		}

		public void OnJoinSession()
		{
			m_FrameJoined = Time.frameCount;

			m_Input.SwitchCurrentActionMap("UI");
			Debug.Log($"OnJoinSession {name}, map: {m_Input.currentActionMap.name}, {m_Input.devices}");

			foreach (var device in m_Input.devices)
			{
				Debug.Log(device);
			}


			//CouchPlayersInput.LogInputDevicePairing();

			//m_Input.currentControlScheme
			//InputUser.PerformPairingWithDevice()
		}

		public void OnLeaveSession()
		{
			// to prevent instant leave because the button "was pressed this frame"
			if (m_FrameJoined + 3 > Time.frameCount)
				return;

			m_Input.SwitchCurrentActionMap("Join Session");
			Debug.Log($"OnLeaveSession {name}, map: {m_Input.currentActionMap.name}");

			//CouchPlayersInput.LogInputDevicePairing();
		}
	}
}
