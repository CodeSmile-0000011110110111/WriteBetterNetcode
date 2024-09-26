// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Settings;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Weapons
{
	[DisallowMultipleComponent]
	public sealed class Weapon : MonoBehaviour
	{
		[SerializeField] private WeaponDataAsset m_WeaponDataAsset;
		[SerializeField] private Transform[] m_ProjectileSpawnPoints;

		public WeaponDataAsset DataAsset => m_WeaponDataAsset;

		private void OnEnable() => m_WeaponDataAsset.Data.RuntimeData.ProjectileSpawnPoints = m_ProjectileSpawnPoints;

		public void FireAudioVisual()
		{
			// foreach (var spawnPoint in m_ProjectileSpawnPoints)
			// 	Debug.Log($"BAMM! {spawnPoint}");
		}
	}
}
