// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeSmile.Components
{
	public class ChildrenRenderOnlyShadows : MonoBehaviour
	{
		private void OnEnable()
		{
			foreach (var renderer in GetComponentsInChildren<Renderer>())
				renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

			Destroy(this);
		}
	}
}
