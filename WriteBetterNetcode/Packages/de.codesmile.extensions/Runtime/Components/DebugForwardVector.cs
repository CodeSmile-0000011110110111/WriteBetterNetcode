// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmile.Components
{
	/// <summary>
	/// Editor-only: shows the current forward vector in the Inspector.
	/// </summary>
	[ExecuteInEditMode]
	public class DebugForwardVector : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField] private Vector3 m_ForwardVector;

		private void OnValidate() => SetForwardVector();
		private void Reset() => SetForwardVector();
		private void Update() => SetForwardVector();
		private void SetForwardVector() => m_ForwardVector = transform.forward;
#endif
	}
}
