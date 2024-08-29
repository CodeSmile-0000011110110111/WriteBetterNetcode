// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Settings
{
	[CreateAssetMenu(fileName = "Kinematic Controllers", menuName = "CodeSmile/Kinematic Controllers", order = 1)]
	public sealed class KinematicControllerPrefabs : ScriptableObject, IEnumerable
	{
		[SerializeField] private List<GameObject> m_Prefabs = new();
		public GameObject this[Int32 index]
		{
			get
			{
				if (index < 0 || index >= m_Prefabs.Count)
				{
					Debug.LogWarning($"Kinematic controller index {index} out of bounds!");
					return null;
				}

				return m_Prefabs[index];
			}
		}
		public Int32 Count => m_Prefabs.Count;
		public IEnumerator GetEnumerator() => m_Prefabs.GetEnumerator();
	}
}
