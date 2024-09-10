// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings.GameStates
{
	[CreateAssetMenu(fileName = nameof(TitleScreenState), order = 1,
		menuName = "CodeSmile/Game States/" + nameof(TitleScreenState))]
	public sealed class TitleScreenState : GameStateBase {}
}
