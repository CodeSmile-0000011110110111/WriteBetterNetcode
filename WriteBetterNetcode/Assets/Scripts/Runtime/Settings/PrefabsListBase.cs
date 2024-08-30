// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Settings
{
	public abstract class PrefabsListBase : ScriptableObject, IEnumerable
	{
		[SerializeField] protected List<GameObject> m_Prefabs = new();
		public GameObject this[Int32 index]
		{
			get
			{
				if (index < 0 || index >= m_Prefabs.Count)
				{
					Debug.LogWarning($"index {index} out of bounds! Returning null");
					return null;
				}

				return m_Prefabs[index];
			}
		}
		public Int32 Count => m_Prefabs.Count;
		public IEnumerator GetEnumerator() => m_Prefabs.GetEnumerator();

		public void ValidatePrefabsHaveComponent<T>() where T : Component
		{
#if DEBUG || DEVELOPMENT_BUILD
			foreach (var prefab in m_Prefabs)
			{
				if (prefab.TryGetComponent<T>(out var _) == false)
					Debug.LogError($"{prefab.name} is missing required {nameof(T)} component");
			}
#endif
		}
	}
}
