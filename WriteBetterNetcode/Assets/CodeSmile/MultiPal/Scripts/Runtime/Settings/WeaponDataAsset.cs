﻿// Copyright (C) 2021-2024 Steffen Itterheim
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
		public ProjectileDataAsset Projectile;
		public Transform[] ProjectileSpawnPoints;

		public Single ReloadDuration;
		public Single FireRate;
		public Int32 AmmoPerShot;
		public Int32 MagazineSize;
	}

	public struct WeaponRuntimeData
	{

		public Single ReloadCompleteTime;
		public Int32 MagazineAmmoRemaining;
	}
}
