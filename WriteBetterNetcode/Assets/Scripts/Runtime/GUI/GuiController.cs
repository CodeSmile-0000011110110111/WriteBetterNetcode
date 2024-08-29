// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using CodeSmile.Input;
using CodeSmile.Players;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.GUI
{
	[DisallowMultipleComponent]
	public sealed class GuiController : MonoBehaviour, GeneratedInput.IPlayerUIActions
	{
		[SerializeField] private DevMainMenu m_MainMenu;
		[SerializeField] private DevIngameMenu m_IngameMenu;

		private CouchPlayers m_CouchPlayers;

		public void OnRequestMenu(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				var userIndex = InputUsers.GetUserIndex(context);
				if (userIndex >= 0)
					OnPlayerRequestIngameMenu(userIndex);
			}
		}

		public void OnPrevious(InputAction.CallbackContext context) {}
		public void OnNext(InputAction.CallbackContext context) {}

		private void Awake()
		{
			ThrowIfNotAssigned<DevMainMenu>(m_MainMenu);
			ThrowIfNotAssigned<DevIngameMenu>(m_IngameMenu);

			CouchPlayers.OnCouchSessionStarted += OnCouchSessionStarted;
			CouchPlayers.OnCouchSessionStopped += OnCouchSessionStopped;
		}

		private void OnDestroy()
		{
			CouchPlayers.OnCouchSessionStarted -= OnCouchSessionStarted;
			CouchPlayers.OnCouchSessionStopped -= OnCouchSessionStopped;
		}

		private void SetInputRequestMenuEnabled(Boolean enabled)
		{
			var inputUsers = Components.InputUsers;
			foreach (var actions in inputUsers.Actions)
			{
				var ingameUi = actions.PlayerUI;
				if (enabled)
				{
					ingameUi.Enable();
					ingameUi.SetCallbacks(this);
				}
				else
				{
					ingameUi.Disable();
					ingameUi.SetCallbacks(null);
				}
			}
		}

		private void OnCouchSessionStarted(CouchPlayers localCouchPlayers)
		{
			m_CouchPlayers = localCouchPlayers;
			m_CouchPlayers.OnCouchPlayerJoin += OnCouchPlayerJoin;
			m_CouchPlayers.OnCouchPlayerLeave += OnCouchPlayerLeave;

			SetInputRequestMenuEnabled(true);
		}

		private void OnCouchSessionStopped() => m_CouchPlayers = null;

		private void OnCouchPlayerJoin(Int32 playerIndex) {}

		private void OnCouchPlayerLeave(Int32 playerIndex)
		{
			// leave from menu? Close menu!
			if (m_IngameMenu.IsVisible && m_IngameMenu.MenuPlayerIndex == playerIndex)
				m_IngameMenu.Hide();
		}

		private void OnPlayerRequestIngameMenu(Int32 playerIndex)
		{
			Debug.Log($"Menu request Player #{playerIndex}");
			m_IngameMenu.MenuPlayerIndex = playerIndex;
			m_IngameMenu.ToggleVisible();
		}

		private void ThrowIfNotAssigned<T>(Component component) where T : Component
		{
			if (component == null || component is not T)
				throw new MissingReferenceException($"{typeof(T).Name} not assigned");
		}
	}
}
