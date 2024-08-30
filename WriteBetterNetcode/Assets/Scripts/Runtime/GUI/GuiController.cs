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

		private CouchPlayers m_CouchPlayers;

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

		private void OnCouchSessionStarted(CouchPlayers localCouchPlayers)
		{
			m_CouchPlayers = localCouchPlayers;
			m_CouchPlayers.OnCouchPlayerJoin += OnCouchPlayerJoin;
			m_CouchPlayers.OnCouchPlayerLeave += OnCouchPlayerLeave;
		}

		private void OnCouchSessionStopped() => m_CouchPlayers = null;

		private void OnCouchPlayerJoin(Int32 playerIndex) =>
			m_CouchPlayers[playerIndex].DidRequestToggleIngameMenu += OnRequestToggleIngameMenu;

		private void OnCouchPlayerLeave(Int32 playerIndex)
		{
			m_CouchPlayers[playerIndex].DidRequestToggleIngameMenu -= OnRequestToggleIngameMenu;

			// leave from menu? Close menu!
			if (m_IngameMenu.IsVisible && m_IngameMenu.MenuPlayerIndex == playerIndex)
				m_IngameMenu.Hide();
		}

		private void OnRequestToggleIngameMenu(Int32 playerIndex)
		{
			m_IngameMenu.MenuPlayerIndex = playerIndex;
			m_IngameMenu.ToggleVisible();

			if (m_IngameMenu.IsVisible)
				m_CouchPlayers[playerIndex].OnOpenIngameMenu();
			else
				m_CouchPlayers[playerIndex].OnCloseIngameMenu();
		}

		private void ThrowIfNotAssigned<T>(Component component) where T : Component
		{
			if (component == null || component is not T)
				throw new MissingReferenceException($"{typeof(T).Name} not assigned");
		}
	}
}
