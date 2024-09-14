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
	public sealed class DevAdvanceState : MonoBehaviour
	{
		[SerializeField] [Range(0f, 10f)] private Single m_SecondsUntilNextScreen = 2f;
		[SerializeField] private Boolean m_AllowSkipWithAnyButton = true;

		[Tooltip("If enabled and running in the editor will advance to next state instantaneously.")]
		[SerializeField] private Boolean m_SkipInPlayMode;
		[SerializeField] private Boolean m_SkipInDevBuilds;

		private Boolean m_IsAnyKeyDown;

		private static void GotoNextState()
		{
			var gameState = ComponentsRegistry.Get<GameState>();
			gameState.AdvanceState();
		}

		private void Start()
		{
			m_IsAnyKeyDown = UnityEngine.Input.anyKey;

			var shouldSkip = m_SkipInPlayMode && Application.isEditor;
#if DEBUG || DEVELOPMENT_BUILD
			shouldSkip = shouldSkip || m_SkipInDevBuilds;
#endif

			// if skipping prevent the screen from flashing the scene's content, it's annoying, distracting, seizure inducing
			if (shouldSkip)
				Camera.main?.gameObject.SetActive(false);

			var seconds = shouldSkip ? 0.56f : m_SecondsUntilNextScreen;
			StartCoroutine(Wait(seconds));
		}

		private void OnValidate() => m_SecondsUntilNextScreen = Mathf.Max(0f, m_SecondsUntilNextScreen);

		private void Update()
		{
			if (m_AllowSkipWithAnyButton)
			{
				if (m_IsAnyKeyDown)
				{
					// force user to release key before another "any key" press is accepted
					m_IsAnyKeyDown = UnityEngine.Input.anyKey;
				}
				else if (UnityEngine.Input.anyKey)
				{
					m_IsAnyKeyDown = true; // prevents calling the next method repeatedly
					GotoNextState();
				}
			}
		}

		private IEnumerator Wait(Single seconds)
		{
			// wait at least for one frame
			if (seconds <= 0f)
				yield return new WaitForEndOfFrame();
			else
				yield return new WaitForSeconds(seconds);

			GotoNextState();
		}
	}
}
