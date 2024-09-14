// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings.GameStates
{
	[CreateAssetMenu(fileName = nameof(LaunchScreenState), order = 0, menuName = MenuRoot + nameof(LaunchScreenState))]
	public sealed class LaunchScreenState : GameStateBase {}
}
