// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	[CreateAssetMenu(fileName = nameof(WeaponDataAsset), menuName = "CodeSmile/" + nameof(WeaponDataAsset), order = 0)]
	public sealed class WeaponDataAsset : ScriptableObject
	{
		public WeaponData Data;
	}

	[Serializable]
	public sealed class WeaponData
	{
		public ProjectileDataAsset Projectile;

		public float ReloadDuration;
		public float FireRate;
		public int AmmoPerShot;
		public int MagazineSize;

		// FIXME: this does not belong inside
		public WeaponRuntimeData RuntimeData;
	}

	public struct WeaponRuntimeData
	{
		public Transform[] ProjectileSpawnPoints;

		public float ReloadCompleteTime;
		public int MagazineAmmoRemaining;
	}
}
