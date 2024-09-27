// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	[CreateAssetMenu(fileName = nameof(PlayerAvatarPrefabs), menuName = "CodeSmile/" + nameof(PlayerAvatarPrefabs), order = 10)]
	public sealed class PlayerAvatarPrefabs : PrefabsListBase {}
}
