// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmile.Components
{
	[RequireComponent(typeof(ParticleSystem))]
	internal class AutoDestroyParticleSystem : MonoBehaviour
	{
		private void OnEnable() => Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
	}
}
