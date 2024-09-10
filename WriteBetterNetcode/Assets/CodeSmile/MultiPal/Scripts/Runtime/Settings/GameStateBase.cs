// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	public abstract class GameStateBase : ScriptableObject
	{
		[SerializeField] private SceneReference[] m_ScenesToLoad;
		[SerializeField] private SceneReference[] m_ScenesToUnload;

		public IReadOnlyList<SceneReference> ScenesToLoad => m_ScenesToLoad;
		public IReadOnlyList<SceneReference> ScenesToUnload => m_ScenesToUnload;
	}
}
