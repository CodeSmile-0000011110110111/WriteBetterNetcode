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
	public sealed class PlayerController : MonoBehaviour, IPlayerComponent, GeneratedInput.IPlayerKinematicsActions
	{
		[SerializeField] private KinematicControllerPrefabs m_ControllerPrefabs;
		[SerializeField] private Transform m_MotionTarget;

		private Int32 m_PlayerIndex;

		private PlayerControllers m_PlayerControllers;
		private PlayerControllerBase ActiveController => m_PlayerControllers.GetActiveController(m_PlayerIndex);

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			m_PlayerIndex = playerIndex;
			m_PlayerControllers = Components.PlayerControllers;

			if (m_MotionTarget == null)
				m_MotionTarget = transform;

			var cameraTarget = GetComponent<PlayerCamera>().Target;
			m_PlayerControllers.InstantiatePlayerControllers(playerIndex, m_ControllerPrefabs, m_MotionTarget, cameraTarget);
		}

		public void OnPlayerDespawn(Int32 playerIndex) => m_PlayerControllers.DestroyPlayerControllers(playerIndex);

		public void OnMove(InputAction.CallbackContext context) => ActiveController.OnMove(context);
		public void OnLook(InputAction.CallbackContext context) => ActiveController.OnLook(context);
		public void OnCrouch(InputAction.CallbackContext context) => ActiveController.OnCrouch(context);
		public void OnJump(InputAction.CallbackContext context) => ActiveController.OnJump(context);
		public void OnSprint(InputAction.CallbackContext context) => ActiveController.OnSprint(context);

		private void Awake()
		{
			if (m_ControllerPrefabs == null)
				throw new MissingReferenceException(nameof(KinematicControllerPrefabs));

			m_ControllerPrefabs.ValidatePrefabsHaveComponent<PlayerControllerBase>();
		}

		public void PreviousController() => m_PlayerControllers.SetPreviousControllerActive(m_PlayerIndex);

		public void NextController() => m_PlayerControllers.SetNextControllerActive(m_PlayerIndex);
	}
}
