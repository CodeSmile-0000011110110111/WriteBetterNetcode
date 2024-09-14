// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace CodeSmile.MultiPal.Settings.GameStates
{
	[CreateAssetMenu(fileName = nameof(OnTimeoutCondition), menuName = GameStateBase.MenuRoot + nameof(OnTimeoutCondition),
		order = 0)]
	public sealed class OnTimeoutCondition : GameStateConditionBase
	{
		[SerializeField] [Range(0f, 10f)] private Single m_SecondsUntilNextScreen = 2f;
		[SerializeField] private Boolean m_AllowSkipWithAnyButton = true;
		[Tooltip("If enabled and running in the editor will advance to next state instantaneously.")]
		[SerializeField] private Boolean m_SkipInPlayMode;
		[SerializeField] private Boolean m_SkipInDevBuilds;

		private Single m_TimeElapsed;
		private void OnValidate() => m_SecondsUntilNextScreen = Mathf.Max(0f, m_SecondsUntilNextScreen);

		public override void OnEnterState()
		{
			m_TimeElapsed = Time.time + m_SecondsUntilNextScreen;

			if (m_AllowSkipWithAnyButton)
				InputSystem.onAnyButtonPress.CallOnce(control => Skip());

#if DEBUG || DEVELOPMENT_BUILD || UNITY_EDITOR
			var shouldSkip = m_SkipInPlayMode && (Application.isEditor || m_SkipInDevBuilds);
			if (shouldSkip)
				Skip();
#endif
		}

		private void Skip() => m_TimeElapsed = 0f;

		public override Boolean IsSatisfied() => Time.time >= m_TimeElapsed;
	}
}
