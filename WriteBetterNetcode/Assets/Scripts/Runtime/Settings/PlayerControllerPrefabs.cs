// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Settings
{
	[CreateAssetMenu(fileName = "Player Controllers", menuName = "CodeSmile/Player Controllers", order = 1)]
	public sealed class PlayerControllerPrefabs : ScriptableObject
	{
		[SerializeField] private List<GameObject> m_Prefabs = new();
		public GameObject this[Byte index] => index >= 0 && index < m_Prefabs.Count ? m_Prefabs[index] : null;
	}
}
