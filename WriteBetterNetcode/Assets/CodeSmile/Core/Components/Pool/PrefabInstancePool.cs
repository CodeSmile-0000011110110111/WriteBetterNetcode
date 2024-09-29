// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace CodeSmile.Components.Pool
{
	public sealed class PrefabInstancePool
	{
		private readonly GameObject m_Prefab;
		private readonly Transform m_Container;
		private readonly ObjectPool<GameObject> m_Pool;

		public GameObject Prefab => m_Prefab;
		public Transform Container => m_Container;

		public int CountActive => m_Pool.CountActive;
		public int CountInactive => m_Pool.CountInactive;
		public int CountAll => m_Pool.CountAll;

		public PrefabInstancePool(GameObject prefab, Transform parent, Int32 initialCapacity = 100,
			Int32 maximumCapacity = 10000)
		{
			if (prefab == null)
				throw new ArgumentNullException(nameof(prefab));

			m_Prefab = prefab;
			m_Container = new GameObject($"{prefab.name} Instances").transform;
			m_Container.parent = parent;
			m_Pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy, true, initialCapacity,
				maximumCapacity);
		}

		public GameObject GetInstance() => m_Pool.Get();
		public void ReleaseInstance(GameObject go) => m_Pool.Release(go);
		public void Clear() => m_Pool.Clear();

		private GameObject OnCreate()
		{
			var go = Object.Instantiate(m_Prefab, m_Container);
			// Debug.Log($"Create instance {go.GetInstanceID()} {go}");

			AutoReturnToPool returnToPool = null;
			if (go.TryGetComponent(out returnToPool) == false)
			{
				returnToPool = go.AddComponent<AutoReturnToPool>();
				returnToPool.Pool = this;
			}

			return go;
		}

		private void OnGet(GameObject go)
		{
			// Debug.Log($"Get instance {go.GetInstanceID()} {go}");
			go.SetActive(true);
		}

		private void OnRelease(GameObject go)
		{
			// Debug.Log($"Release instance {go.GetInstanceID()} {go}");
			go.SetActive(false);
		}

		private void OnDestroy(GameObject go)
		{
			// Debug.Log($"Destroy instance {go.GetInstanceID()} {go}");
		}
	}
}
