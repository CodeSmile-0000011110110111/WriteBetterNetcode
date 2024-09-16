// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	internal sealed class PingOnEnterPlaymode : MonoBehaviour
	{
		private void Start()
		{
#if UNITY_EDITOR
			EditorGUIUtility.PingObject(gameObject);
#else
			Destroy(this);
#endif
		}
	}
}
