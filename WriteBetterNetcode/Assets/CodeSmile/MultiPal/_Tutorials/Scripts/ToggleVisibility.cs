// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal._Tutorials.Scripts
{
	public class ToggleVisibility : MonoBehaviour
	{
		private void Update()
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
			{
				var renderers = GetComponentsInChildren<Renderer>();
				foreach (var renderer in renderers)
					renderer.enabled = !renderer.enabled;
			}
		}
	}
}
