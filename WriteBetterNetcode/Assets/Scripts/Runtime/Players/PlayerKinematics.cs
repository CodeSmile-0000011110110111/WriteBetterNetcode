// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using CodeSmile.Players.Controllers;
using CodeSmile.Settings;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	public sealed class PlayerKinematics : MonoBehaviour, IPlayerComponent, GeneratedInput.IPlayerKinematicsActions
	{
		[SerializeField] private KinematicControllerPrefabs m_ControllerPrefabs;

		private Int32 m_PlayerIndex;
		private Int32 m_ActiveIndex;
		private KinematicControllerBase[] m_Controllers;
		private KinematicControllerBase ActiveController => GetOrCreateController();

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;
			SetControllerActive(m_ActiveIndex);
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			foreach (var controller in m_Controllers)
				controller?.OnDeactivate(playerIndex);
		}

		public void OnMove(InputAction.CallbackContext context) => ActiveController.OnMove(context);
		public void OnLook(InputAction.CallbackContext context) => ActiveController.OnLook(context);
		public void OnCrouch(InputAction.CallbackContext context) => ActiveController.OnCrouch(context);
		public void OnJump(InputAction.CallbackContext context) => ActiveController.OnJump(context);
		public void OnSprint(InputAction.CallbackContext context) => ActiveController.OnSprint(context);

		private void Awake()
		{
			if (m_ControllerPrefabs == null)
				throw new MissingComponentException(nameof(KinematicControllerPrefabs));

			m_Controllers = new KinematicControllerBase[m_ControllerPrefabs.Count];
		}

		private KinematicControllerBase GetOrCreateController()
		{
			if (m_Controllers[m_ActiveIndex] == null)
			{
				var prefab = m_ControllerPrefabs[m_ActiveIndex];
				var obj = Instantiate(prefab, transform);

				var controller = obj.GetComponent<KinematicControllerBase>();
				if (controller == null)
					throw new MissingComponentException($"{prefab.name}: missing {nameof(KinematicControllerBase)}");

				m_Controllers[m_ActiveIndex] = controller;
				controller.OnActivate(m_PlayerIndex);
			}

			return m_Controllers[m_ActiveIndex];
		}

		public void SetControllerActive(Int32 controllerIndex)
		{
			if (controllerIndex != m_ActiveIndex)
			{
				ActiveController.gameObject.SetActive(false);
				m_ActiveIndex = controllerIndex;
			}

			ActiveController.gameObject.SetActive(true);
		}

		public void PreviousController() =>
			SetControllerActive(m_ActiveIndex == 0 ? m_ControllerPrefabs.Count - 1 : m_ActiveIndex - 1);

		public void NextController() =>
			SetControllerActive(m_ActiveIndex == m_ControllerPrefabs.Count - 1 ? 0 : m_ActiveIndex + 1);
	}
}
