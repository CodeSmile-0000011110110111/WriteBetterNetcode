// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Players;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GUI
{
	[DisallowMultipleComponent]
	public sealed class GuiController : MonoBehaviour
	{
		[SerializeField] private DevMainMenu m_MainMenu;
		[SerializeField] private DevIngameMenu m_IngameMenu;

		private void Awake()
		{
			ThrowIfNotAssigned<DevMainMenu>(m_MainMenu);
			ThrowIfNotAssigned<DevIngameMenu>(m_IngameMenu);

			Components.OnLocalCouchPlayersSpawn += OnCouchSessionStarted;
			Components.OnLocalCouchPlayersDespawn += OnCouchSessionStopped;
		}

		private void OnDestroy()
		{
			Components.OnLocalCouchPlayersSpawn -= OnCouchSessionStarted;
			Components.OnLocalCouchPlayersDespawn -= OnCouchSessionStopped;
		}

		private void OnCouchSessionStarted(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoined += OnCouchPlayerJoined;
			couchPlayers.OnCouchPlayerLeaving += OnCouchPlayerLeaving;
		}

		private void OnCouchSessionStopped(CouchPlayers couchPlayers)
		{
			if (m_IngameMenu.IsVisible)
				m_IngameMenu.Hide();
		}

		private void OnCouchPlayerJoined(CouchPlayers couchPlayers, Int32 playerIndex) =>
			couchPlayers[playerIndex].OnRequestToggleIngameMenu += OnRequestToggleIngameMenu;

		private void OnCouchPlayerLeaving(CouchPlayers couchPlayers, Int32 playerIndex)
		{
			couchPlayers[playerIndex].OnRequestToggleIngameMenu -= OnRequestToggleIngameMenu;

			// leave from menu? Close menu!
			if (m_IngameMenu.IsVisible && m_IngameMenu.MenuPlayerIndex == playerIndex)
				OnRequestToggleIngameMenu(playerIndex);
		}

		private void OnRequestToggleIngameMenu(Int32 playerIndex)
		{
			m_IngameMenu.MenuPlayerIndex = playerIndex;
			m_IngameMenu.ToggleVisible();

			var couchPlayers = Components.LocalCouchPlayers;
			if (m_IngameMenu.IsVisible)
				couchPlayers[playerIndex].OnOpenIngameMenu();
			else
				couchPlayers[playerIndex].OnCloseIngameMenu();
		}

		private void ThrowIfNotAssigned<T>(Component component) where T : Component
		{
			if (component == null || component is not T)
				throw new MissingReferenceException($"{typeof(T).Name} not assigned");
		}
	}
}
