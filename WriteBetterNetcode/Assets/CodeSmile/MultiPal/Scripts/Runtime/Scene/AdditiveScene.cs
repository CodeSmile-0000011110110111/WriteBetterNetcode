// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Utility;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Scene
{
	[Serializable]
	public sealed class AdditiveScene
	{
		[SerializeField] private SceneReference m_SceneReference;
		[Tooltip("If enabled and the scene is already loaded, it will be unloaded and loaded again to reset its state. " +
		         "Default is to leave already loaded scenes intact.")]
		[SerializeField] private Boolean m_ForceReload;
		// [Tooltip("If enabled, bypasses the 'scene already loaded' check thus allowing multiple identical scenes to be " +
		//          "loaded simulataneously. Use with caution.")]
		// [SerializeField] private Boolean m_AllowMultiple = false;

		public SceneReference Reference => m_SceneReference;
		public Boolean ForceReload => m_ForceReload;
	}
}
