// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

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

		private void Start()
		{
			m_Input = GetComponent<PlayerInput>();
			m_Input.onActionTriggered += OnInputAction;
		}

		private void OnInputAction(InputAction.CallbackContext context)
		{
			//Debug.Log($"OnInputAction: {context.action.name} map:{context.action.actionMap.name}");
		}

		public void OnLook(InputValue dir)
		{
			//Debug.Log($"OnLook: {dir.Get<Vector2>()}");
		}
	}
}
