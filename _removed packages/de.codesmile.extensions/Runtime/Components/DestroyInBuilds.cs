// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmile.Components
{
	public class DestroyInBuilds : MonoBehaviour
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
