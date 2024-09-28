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
	public sealed class Weapon : MonoBehaviour
	{
		public event Action<Weapon> OnReloadStarted;
		public event Action<Weapon> OnReloadComplete;
		public event Action<Weapon> OnProjectileSpawn;
		[SerializeField] private WeaponDataAsset m_WeaponDataAsset;
		[SerializeField] private Transform[] m_ProjectileSpawnPoints;

		private ProjectileSpawner m_ProjectileSpawner;
		private WeaponData m_Data;
		private WeaponRuntimeData m_RuntimeData;

		private void Awake()
		{
			m_ProjectileSpawner = ComponentsRegistry.Get<ProjectileSpawner>();
			m_Data = m_WeaponDataAsset.Data;
			m_RuntimeData = default;
		}

		private void Update()
		{
			ReloadUpdate();
			AttackUpdate();
		}

		private void ReloadUpdate()
		{
			if (m_RuntimeData.IsReloading)
			{
				if (m_RuntimeData.ReloadCompleteTime >= Time.time)
				{
					m_RuntimeData.IsReloading = false;
					m_RuntimeData.MagazineAmmoRemaining = m_Data.MagazineSize;
					OnReloadComplete?.Invoke(this);
				}
			}
		}

		private void AttackUpdate()
		{
			if (m_RuntimeData.IsReloading)
				return;

			if (m_RuntimeData.IsAttacking)
			{
				var timeNow = Time.time;
				if (m_RuntimeData.NextShotTime <= timeNow)
				{
					var shotCount = Mathf.Max(m_Data.ShotCount, m_RuntimeData.MagazineAmmoRemaining);
					if (shotCount > 0)
					{
						m_RuntimeData.MagazineAmmoRemaining -= shotCount;
						m_RuntimeData.NextShotTime = m_Data.FireRate + timeNow;

						m_ProjectileSpawner.Spawn(m_Data, m_ProjectileSpawnPoints);
						OnProjectileSpawn?.Invoke(this);

						if (m_Data.WeaponFirePrefab != null)
						{
							foreach (var spawnPoint in m_ProjectileSpawnPoints)
								Instantiate(m_Data.WeaponFirePrefab, spawnPoint.position, spawnPoint.rotation, transform);
						}
					}
					else if (m_Data.AutoReload)
						Reload();
				}
			}
		}

		private void Reload()
		{
			m_RuntimeData.IsReloading = true;
			m_RuntimeData.ReloadCompleteTime = m_Data.ReloadDuration + Time.time;
			OnReloadStarted?.Invoke(this);
		}

		public void StartAttacking()
		{
			if (m_RuntimeData.IsAttacking == false)
				m_RuntimeData.IsAttacking = true;
		}

		public void StopAttacking() => m_RuntimeData.IsAttacking = false;
	}
}
