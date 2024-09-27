﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	[CreateAssetMenu(fileName = nameof(WeaponPrefabs), menuName = "CodeSmile/Weapons/" + nameof(WeaponPrefabs))]
	public sealed class WeaponPrefabs : PrefabsListBase {}
}
