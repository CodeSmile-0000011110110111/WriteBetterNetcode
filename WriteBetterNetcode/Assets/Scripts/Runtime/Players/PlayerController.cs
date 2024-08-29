// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Settings;
using System;
using UnityEngine;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	public sealed class PlayerController : MonoBehaviour
	{
		[SerializeField] private PlayerControllerPrefabs m_ControllerPrefabs;
		[SerializeField] private int m_ActiveControllerIndex;

		private ModularKinematicControllerBase m_ActiveController;

		private void Awake()
		{
			if (m_ControllerPrefabs == null)
				throw new MissingComponentException(nameof(PlayerControllerPrefabs));
		}

		public void OnPlayerSpawn(int playerIndex)
		{
			SetController(m_ActiveControllerIndex);
			m_ActiveController?.OnPlayerSpawn(playerIndex);
		}

		public void OnPlayerDespawn(int playerIndex)
		{
			m_ActiveController?.OnPlayerDespawn(playerIndex);
			Destroy(m_ActiveController);
			m_ActiveController = null;
		}

		public void SetController(int controllerIndex)
		{
			var prefab = m_ControllerPrefabs[controllerIndex];
			if (prefab == null)
				throw new ArgumentNullException($"PlayerControllerPrefab[{controllerIndex}] is null");

			if (m_ActiveController != null)
				Destroy(m_ActiveController);

			var controllerObj = Instantiate(prefab, transform);
			m_ActiveController = controllerObj.GetComponent<ModularKinematicControllerBase>();
			if (m_ActiveController == null)
				throw new MissingComponentException($"{controllerObj.name}: missing {nameof(ModularKinematicControllerBase)}");
		}
	}
}
