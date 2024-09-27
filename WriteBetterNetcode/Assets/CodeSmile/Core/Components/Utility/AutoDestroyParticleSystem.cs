﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	[RequireComponent(typeof(ParticleSystem))]
	internal sealed class AutoDestroyParticleSystem : MonoBehaviour
	{
		private void OnEnable() => Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
	}
}
