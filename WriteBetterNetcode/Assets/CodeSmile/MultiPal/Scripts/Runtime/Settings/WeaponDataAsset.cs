// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	[CreateAssetMenu(fileName = nameof(WeaponDataAsset), menuName = "CodeSmile/Weapons/" + nameof(WeaponDataAsset), order = 1)]
	public sealed class WeaponDataAsset : ScriptableObject
	{
		public WeaponData Data;
	}

	[Serializable]
	public sealed class WeaponData
	{
		[Header("Projectiles")]
		public ProjectileDataAsset Projectile;

		[Header("AudioVisual")]
		public GameObject WeaponFirePrefab;

		[Header("Settings")]
		public Single FireRate;

		[Header("Ammo")]
		[Tooltip("How many projectiles to fire per shot.")]
		public Int32 ShotCount;
		[Tooltip("Number of projectiles in the magazine/chamber. If this drops to zero a weapon reload is required.")]
		public Int32 MagazineSize;

		[Header("Reload")]
		[Tooltip("Auto reload weapon if magazine empty. If false, reload requires user interaction.")]
		public bool AutoReload = true;
		[Tooltip("How long reloading takes, in seconds.")]
		public Single ReloadDuration;
	}

	public struct WeaponRuntimeData
	{
		public bool IsAttacking;
		public bool IsReloading;
		public Single NextShotTime;
		public Single ReloadCompleteTime;
		public Int32 MagazineAmmoRemaining;
	}
}
