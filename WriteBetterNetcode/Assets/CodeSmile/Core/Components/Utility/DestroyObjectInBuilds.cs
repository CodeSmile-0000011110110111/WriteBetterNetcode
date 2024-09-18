﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	/// <summary>
	/// Destroys the object if running outside the editor.
	/// </summary>
	[DisallowMultipleComponent]
	internal sealed class DestroyObjectInBuilds : MonoBehaviour
	{
#if !UNITY_EDITOR
		private void Awake()
		{
			if (enabled)
				Destroy(gameObject);
		}
#endif
	}
}
