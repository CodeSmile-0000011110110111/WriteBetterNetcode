// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	[CreateAssetMenu(fileName = nameof(PlayerCameraPrefabs), menuName = "CodeSmile/" + nameof(PlayerCameraPrefabs), order = 20)]
	public sealed class PlayerCameraPrefabs : PrefabsListBase {}
}
