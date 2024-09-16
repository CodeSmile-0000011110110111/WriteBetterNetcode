﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	[DisallowMultipleComponent]
	internal sealed class DestroyObjectOnAwake : MonoBehaviour
	{
		private void Awake() => Destroy(gameObject);
	}
}
