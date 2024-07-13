// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components
{
	public class SyncPositionWithTarget : MonoBehaviour
	{
		public Transform Target;

		private void Update() => transform.position = Target != null ? Target.position : Vector3.zero;
	}
}
