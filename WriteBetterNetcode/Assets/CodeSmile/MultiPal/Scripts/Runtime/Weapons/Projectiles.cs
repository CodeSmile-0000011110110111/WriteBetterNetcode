// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Weapons
{
	[DisallowMultipleComponent]
	public sealed class Projectiles : MonoBehaviour
	{
		List<ActiveProjectile> m_Projectiles = new();

		private void Awake() => ComponentsRegistry.Set(this);

		public void SpawnProjectile(GameObject projectilePrefab, Vector3 position, Quaternion rotation)
		{
			var projectile = Instantiate(projectilePrefab, position, rotation);
		}

		public void FireWeapon(WeaponData weaponData)
		{
			var projectileData = weaponData.Projectile.Data;
			var projectileRuntimeData = projectileData.RuntimeData;
			projectileRuntimeData.TimeToDie = Time.time + projectileData.MaxLifetime;

			var parent = weaponData.RuntimeData.ProjectileSpawnPoints[0];
			var projectileObj = Instantiate(projectileData.ProjectilePrefab, parent.position, parent.rotation);
			projectileObj.transform.parent = transform;

			m_Projectiles.Add(new ActiveProjectile()
			{
				Transform = projectileObj.transform,
				Data = projectileData,
				RuntimeData = projectileRuntimeData,
			});
		}

		private void Update()
		{
			var currentTime = Time.time;
			var deltaTime = Time.deltaTime;

			var projectileCount = m_Projectiles.Count;
			for (int i = projectileCount - 1; i >= 0; i--)
			{
				var projectile = m_Projectiles[i];
				var runtimeData = projectile.RuntimeData;
				if (runtimeData.TimeToDie > currentTime)
				{
					var projectileTransform = projectile.Transform;
					projectileTransform.position += projectileTransform.forward * (projectile.Data.Speed * deltaTime);
				}
				else
				{
					Destroy(projectile.Transform.gameObject);
					m_Projectiles.RemoveAt(i);
				}
			}
		}

		private struct ActiveProjectile
		{
			public Transform Transform;
			public ProjectileData Data;
			public ProjectileRuntimeData RuntimeData;
		}
	}
}
