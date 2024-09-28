// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Settings;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Weapons
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ProjectileUpdater))]
	public sealed class ProjectileSpawner : MonoBehaviour
	{
		private readonly ActiveProjectiles m_Projectiles = new();
		internal ActiveProjectiles Projectiles => m_Projectiles;

		private void Awake() => ComponentsRegistry.Set(this);

		public void Spawn(WeaponData weaponData, Transform[] spawnPoints)
		{
			if (spawnPoints.Length != 1)
				throw new ArgumentException("Spawn points must be exactly 1 for now!");

			var projectileData = weaponData.Projectile.Data;
			var projectileRuntimeData = projectileData.RuntimeData;
			projectileRuntimeData.TimeToDie = Time.time + projectileData.Lifetime;

			var spawn = spawnPoints[0];
			var go = Instantiate(projectileData.ProjectilePrefab, spawn.position, spawn.rotation, transform);

			m_Projectiles.Add(new()
			{
				Transform = go.transform,
				Data = projectileData,
				RuntimeData = projectileRuntimeData,
			});
		}

		internal void DestroyProjectile(ActiveProjectile projectile) => Destroy(projectile.Transform.gameObject);
	}
}
