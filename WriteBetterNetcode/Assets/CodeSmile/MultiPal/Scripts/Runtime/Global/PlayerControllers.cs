// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Player;
using CodeSmile.MultiPal.Settings;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Global
{
	[DisallowMultipleComponent]
	public sealed class PlayerControllers : MonoBehaviour
	{
		private readonly List<PlayerControllerBase>[] m_Controllers =
			new List<PlayerControllerBase>[Constants.MaxCouchPlayers];

		private readonly Int32[] m_ActiveControllers = new Int32[Constants.MaxCouchPlayers];

		private void Awake() => AllocPlayerControllersLists();

		public PlayerControllerBase GetActiveController(Int32 playerIndex)
		{
			var activeIndex = m_ActiveControllers[playerIndex];
			return activeIndex >= 0 ? m_Controllers[playerIndex][activeIndex] : null;
		}

		private void AllocPlayerControllersLists()
		{
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				m_Controllers[playerIndex] = new List<PlayerControllerBase>();
				m_ActiveControllers[playerIndex] = -1;
			}
		}

		public void InstantiatePlayerControllers(Int32 playerIndex, PlayerControllerPrefabs prefabs, Transform motionTarget, Transform rotationTarget)
		{
			for (var ctrlIndex = 0; ctrlIndex < prefabs.Count; ctrlIndex++)
			{
				var ctrlPrefab = prefabs[ctrlIndex];
				var ctrlObj = Instantiate(ctrlPrefab, transform);
				ctrlObj.name = $"Player #{playerIndex}: {ctrlObj.name.Replace("(Clone)", $"[{ctrlIndex}]")}";
				ctrlObj.SetActive(false);

				var controller = ctrlObj.GetComponent<PlayerControllerBase>();
				controller.MotionTarget = motionTarget;
				controller.CameraTarget = rotationTarget;
				m_Controllers[playerIndex].Add(controller);

				if (ctrlIndex == 0)
					SetControllerActive(playerIndex, ctrlIndex);
			}
		}

		public void DestroyPlayerControllers(Int32 playerIndex)
		{
			foreach (var controller in m_Controllers[playerIndex])
				Destroy(controller.gameObject);

			m_Controllers[playerIndex].Clear();
			m_ActiveControllers[playerIndex] = -1;
		}

		public void SetControllerActive(Int32 playerIndex, Int32 controllerIndex)
		{
			// check if already active
			if (m_ActiveControllers[playerIndex] == controllerIndex)
				return;

			// deactivate current, activate new one
			GetActiveController(playerIndex)?.gameObject.SetActive(false);
			m_ActiveControllers[playerIndex] = controllerIndex;
			GetActiveController(playerIndex)?.gameObject.SetActive(true);
		}

		public void SetPreviousControllerActive(Int32 playerIndex) => SetControllerActive(playerIndex,
			m_ActiveControllers[playerIndex] == 0
				? m_Controllers[playerIndex].Count - 1
				: m_ActiveControllers[playerIndex] - 1);

		public void SetNextControllerActive(Int32 playerIndex) => SetControllerActive(playerIndex,
			m_ActiveControllers[playerIndex] == m_Controllers[playerIndex].Count - 1
				? 0
				: m_ActiveControllers[playerIndex] + 1);
	}
}
