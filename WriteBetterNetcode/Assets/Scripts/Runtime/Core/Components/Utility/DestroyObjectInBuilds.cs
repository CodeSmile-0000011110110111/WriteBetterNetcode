// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Components.Utility
{
	public class DestroyObjectInBuilds : MonoBehaviour
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
