// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	[CreateAssetMenu(fileName = nameof(PlayerControllerPrefabs), menuName = "CodeSmile/" + nameof(PlayerControllerPrefabs), order = 30)]
	public sealed class PlayerControllerPrefabs : PrefabsListBase {}
}
