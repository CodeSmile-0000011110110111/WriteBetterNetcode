// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Pool
{
	[DisallowMultipleComponent]
	public sealed class PrefabPool : MonoBehaviour
	{
		[SerializeField] [Range(10, 10000)] private Int32 m_InitialCapacity = 100;
		[SerializeField] [Range(10, 10000)] private Int32 m_MaxCapacity = 10000;

		private readonly Dictionary<Int32, PrefabInstancePool> m_Pools = new();

		private void Awake() => ComponentsRegistry.Set(this);

		private void Start() => StartCoroutine(UpdatePoolNames());

		private IEnumerator UpdatePoolNames()
		{
#if UNITY_EDITOR
			while (true)
			{
				yield return new WaitForSeconds(m_StatsUpdateFrequency);

				var activeTotal = 0;
				var inactiveTotal = 0;
				var allTotal = 0;

				if (m_ShowPoolStats)
				{
					foreach (var pool in m_Pools.Values)
					{
						var active = pool.CountActive;
						var inactive = pool.CountInactive;
						var all = pool.CountAll;
						pool.Container.name = $"{pool.Prefab.name} (Active: {active}, Inactive: {inactive}, Total: {all})";

						activeTotal += active;
						inactiveTotal += inactive;
						allTotal += all;
					}

					name = $"{nameof(PrefabPool)}  (Active: {activeTotal}, Inactive: {inactiveTotal}, Total: {allTotal})";
				}
			}
#endif
		}

		public GameObject GetInstance(GameObject prefab)
		{
			var pool = GetOrCreatePool(prefab);
			return pool.GetInstance();
		}

		public GameObject GetInstance(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			var pool = GetOrCreatePool(prefab);
			var instance = pool.GetInstance();
			instance.transform.SetPositionAndRotation(position, rotation);
			return instance;
		}

		public PrefabInstancePool CreatePool(GameObject prefab, Int32 initialCapacity, Int32 maxCapacity)
		{
			var pool = GetPool(prefab);
			if (pool != null)
				DisposePool(prefab);

			initialCapacity = initialCapacity <= 0 ? m_InitialCapacity : initialCapacity;
			maxCapacity = maxCapacity <= 0 ? m_MaxCapacity : maxCapacity;
			CreatePool(prefab, prefab.GetInstanceID(), initialCapacity, maxCapacity);

			return GetPool(prefab);
		}

		public void ClearPool(GameObject prefab)
		{
			var pool = GetOrCreatePool(prefab);
			pool.Clear();
		}

		public void DisposePool(GameObject prefab)
		{
			var pool = GetOrCreatePool(prefab);
			m_Pools[prefab.GetInstanceID()] = null;

			pool.Clear();
			Destroy(pool.Container);
		}

		public PrefabInstancePool GetPool(GameObject prefab) => m_Pools[prefab.GetInstanceID()];

		public PrefabInstancePool GetOrCreatePool(GameObject prefab)
		{
			var prefabId = prefab.GetInstanceID();
			if (m_Pools.ContainsKey(prefabId) == false)
				CreatePool(prefab, prefabId, m_InitialCapacity, m_MaxCapacity);

			return m_Pools[prefabId];
		}

		private void CreatePool(GameObject prefab, Int32 prefabId, Int32 initialCapacity, Int32 maxCapacity) =>
			m_Pools[prefabId] = new PrefabInstancePool(prefab, transform, initialCapacity, maxCapacity);

#if UNITY_EDITOR
		[Header("Debug")]
		[Tooltip("Updates container names frequently with pool statistics. Editor-only.")]
		[SerializeField] private Boolean m_ShowPoolStats;
		[SerializeField] [Range(0.05f, 1f)] private Single m_StatsUpdateFrequency = 0.2f;
#endif
	}
}
