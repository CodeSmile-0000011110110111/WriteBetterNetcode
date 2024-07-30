// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.SceneTools
{
	public class SceneAutoLoader : OneTimeTaskBehaviour
	{
		[SerializeField] private SceneReference m_SceneToLoad;
		[SerializeField] private Single m_TimeToWaitBeforeLoad;

		public static void DestroyAll()
		{
#if UNITY_2022_3_OR_NEWER
			foreach (var loader in FindObjectsByType<SceneAutoLoader>(FindObjectsSortMode.None))
				loader.Disable();
#else
			throw new NotSupportedException("not available in this Unity version");
#endif
		}

		public static void LoadScene()
		{
#if UNITY_2022_3_OR_NEWER
			var loader = FindAnyObjectByType<SceneAutoLoader>();
			if (loader != null)
				loader.LoadSceneInternal();
#else
			throw new NotSupportedException("not available in this Unity version");
#endif
		}

		private void OnValidate()
		{
			m_SceneToLoad?.OnValidate();
			m_TimeToWaitBeforeLoad = Mathf.Max(0f, m_TimeToWaitBeforeLoad);
		}

		private void OnDestroy() => StopAllCoroutines();

		private void Start()
		{
			if (String.IsNullOrWhiteSpace(m_SceneToLoad?.SceneName))
				throw new ArgumentException($"{nameof(SceneAutoLoader)}: scene not assigned");

			StartCoroutine(LoadSceneAfterDelay(m_TimeToWaitBeforeLoad));
		}

		private IEnumerator LoadSceneAfterDelay(Single timeToWaitBeforeLoad)
		{
			yield return new WaitForSeconds(m_TimeToWaitBeforeLoad);

			LoadSceneInternal();
		}

		public void LoadSceneInternal()
		{
			//Debug.Log($"{nameof(SceneAutoLoader)}: loading {m_SceneToLoad.SceneName}");
			SceneManager.LoadScene(m_SceneToLoad.SceneName, LoadSceneMode.Single);
			StopAllCoroutines();
			TaskPerformed();
		}

		public void Disable()
		{
			enabled = false;
			TaskPerformed();
		}
	}
}
