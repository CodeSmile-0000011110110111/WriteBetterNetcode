// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Players.Controllers;
using CodeSmile.Settings;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	public sealed class PlayerKinematics : MonoBehaviour, IPlayerComponent
	{
		[SerializeField] private KinematicControllerPrefabs m_ControllerPrefabs;

		private Int32 m_ActiveControllerIndex;
		private KinematicControllerBase[] m_Controllers;
		private KinematicControllerBase ActiveController => m_Controllers[m_ActiveControllerIndex];

		public void OnPlayerSpawn(Int32 playerIndex)
		{
			InstantiateAllControllers(playerIndex);
			SetControllerActive(m_ActiveControllerIndex);
		}

		public void OnPlayerDespawn(Int32 playerIndex)
		{
			foreach (var controller in m_Controllers)
				controller.OnPlayerDespawn(playerIndex);
		}

		private void Awake()
		{
			if (m_ControllerPrefabs == null)
				throw new MissingComponentException(nameof(KinematicControllerPrefabs));
		}

		private void InstantiateAllControllers(int playerIndex)
		{
			m_Controllers = new KinematicControllerBase[m_ControllerPrefabs.Count];

			for (var i = 0; i < m_ControllerPrefabs.Count; i++)
			{
				var prefab = m_ControllerPrefabs[i];
				var obj = Instantiate(prefab, transform);

				m_Controllers[i] = obj.GetComponent<KinematicControllerBase>();
				if (m_Controllers[i] == null)
					throw new MissingComponentException($"{m_Controllers[i].name}: missing {nameof(KinematicControllerBase)}");

				m_Controllers[i].OnPlayerSpawn(playerIndex);
				m_Controllers[i].gameObject.SetActive(false);
			}
		}

		public void SetControllerActive(Int32 controllerIndex)
		{
			ActiveController.gameObject.SetActive(false);
			m_ActiveControllerIndex = controllerIndex;
			ActiveController.gameObject.SetActive(true);
		}

		public void PreviousController() =>
			SetControllerActive(m_ActiveControllerIndex == 0 ? m_ControllerPrefabs.Count - 1 : m_ActiveControllerIndex - 1);

		public void NextController() =>
			SetControllerActive(m_ActiveControllerIndex == m_ControllerPrefabs.Count - 1 ? 0 : m_ActiveControllerIndex + 1);
	}
}
