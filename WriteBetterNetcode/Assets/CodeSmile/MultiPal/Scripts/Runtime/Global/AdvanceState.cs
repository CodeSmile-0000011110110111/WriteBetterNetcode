// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Global
{
	[DisallowMultipleComponent]
	public sealed class AdvanceState : MonoBehaviour
	{
		[SerializeField] [Range(0f, 30f)] private Single m_SecondsToWait = 1f;
		private void Start() => StartCoroutine(Wait());
		private void OnValidate() => m_SecondsToWait = Mathf.Max(0f, m_SecondsToWait);

		private IEnumerator Wait()
		{
			yield return new WaitForSeconds(m_SecondsToWait);

			var gameState = ComponentsRegistry.Get<GameState>();
			gameState.AdvanceState();
		}
	}
}
