// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerInput))]
	public class TestInput : MonoBehaviour
	{
		private PlayerInput m_Input;

		private Boolean m_Joining;

		private void Start()
		{
			m_Input = GetComponent<PlayerInput>();
			m_Input.SwitchCurrentActionMap("Join Session");
		}

		public void OnLook(InputValue dir)
		{
			//Debug.Log($"OnLook: {dir.Get<Vector2>()}");
		}

		public void OnJoinSession()
		{
			m_Joining = true;

			m_Input.SwitchCurrentActionMap("UI");
			Debug.Log($"OnJoinSession {name}, map: {m_Input.currentActionMap.name}");

			StartCoroutine(WaitJoin());
		}

		public void OnLeaveSession()
		{
			if (m_Joining)
				return;

			m_Input.SwitchCurrentActionMap("Join Session");
			Debug.Log($"OnLeaveSession {name}, map: {m_Input.currentActionMap.name}");
		}

		// workaround for insta-leave when using the same button, and only for the first time switching maps
		private IEnumerator WaitJoin()
		{
			yield return null;

			m_Joining = false;
		}
	}
}
