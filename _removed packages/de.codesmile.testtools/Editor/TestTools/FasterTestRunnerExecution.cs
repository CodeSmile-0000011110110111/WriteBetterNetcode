// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace CodeSmileEditor.TestTools
{
	[InitializeOnLoad] [ExcludeFromCodeCoverage]
	public static class FasterTestRunnerExecution
	{
		static FasterTestRunnerExecution() =>
			ScriptableObject.CreateInstance<TestRunnerApi>().RegisterCallbacks(new Callbacks());

		[ExcludeFromCodeCoverage]
		private sealed class Callbacks : ICallbacks
		{
			private const String ApplicationIdleTimeKey = "ApplicationIdleTime";
			private const String InteractionModeKey = "InteractionMode";

			private Int32 m_UserApplicationIdleTime;
			private Int32 m_UserInteractionMode;

			private static void UpdateInteractionModeSettings()
			{
				const String UpdateInteractionModeMethodName = "UpdateInteractionModeSettings";

				var bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;
				var type = typeof(EditorApplication);
				var method = type.GetMethod(UpdateInteractionModeMethodName, bindingFlags);
				method.Invoke(null, null);
			}

			private static void SetInteractionModeToNoThrottling()
			{
				EditorPrefs.SetInt(ApplicationIdleTimeKey, 0);
				EditorPrefs.SetInt(InteractionModeKey, 1);
			}

			public void RunStarted(ITestAdaptor testsToRun) => SetInteractionModeToSpeedUpTestRun();
			public void RunFinished(ITestResultAdaptor result) => ResetInteractionMode();
			public void TestStarted(ITestAdaptor test) {}
			public void TestFinished(ITestResultAdaptor result) {}

			private void SetInteractionModeToSpeedUpTestRun()
			{
				//Debug.Log("Set Interaction Mode to 'No Throttling' during tests.");
				GetUserInteractionModeSettings();
				SetInteractionModeToNoThrottling();
				UpdateInteractionModeSettings();
			}

			private void ResetInteractionMode()
			{
				SetInteractionModeToUserSettings();
				UpdateInteractionModeSettings();
				//Debug.Log("Reset Interaction Mode to user setting.");
			}

			private void GetUserInteractionModeSettings()
			{
				m_UserApplicationIdleTime = EditorPrefs.GetInt(ApplicationIdleTimeKey);
				m_UserInteractionMode = EditorPrefs.GetInt(InteractionModeKey);
			}

			private void SetInteractionModeToUserSettings()
			{
				EditorPrefs.SetInt(ApplicationIdleTimeKey, m_UserApplicationIdleTime);
				EditorPrefs.SetInt(InteractionModeKey, m_UserInteractionMode);
			}
		}
	}
}
