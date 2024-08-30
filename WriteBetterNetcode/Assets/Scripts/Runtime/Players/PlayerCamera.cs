// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Settings;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Players
{
	[DisallowMultipleComponent]
	public sealed class PlayerCamera : MonoBehaviour
	{
		[SerializeField] private PlayerCameraPrefabs m_CameraPrefabs;

		private void Awake()
		{
			if (m_CameraPrefabs == null)
				throw new MissingReferenceException(nameof(PlayerCameraPrefabs));

			m_CameraPrefabs.ValidatePrefabsHaveComponent<CinemachineCamera>();
		}
	}
}
