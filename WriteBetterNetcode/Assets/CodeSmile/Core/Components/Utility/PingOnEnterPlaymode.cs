// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	/// <summary>
	///     Pings (expands and highlights) the object when entering playmode (editor-only).
	///     Useful mostly to ensure the hierarchy is always expanded to a specific (possibly deeply nested) object.
	/// </summary>
	[DisallowMultipleComponent]
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
