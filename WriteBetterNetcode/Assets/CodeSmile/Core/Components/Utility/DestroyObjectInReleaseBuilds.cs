// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	[DisallowMultipleComponent]
	internal sealed class DestroyObjectInReleaseBuilds : MonoBehaviour
	{
#if !UNITY_EDITOR && !DEBUG && !DEVELOPMENT_BUILD
		private void Awake()
		{
			if (enabled)
				Destroy(gameObject);
		}
#endif
	}
}
