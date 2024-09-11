// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Players.Couch;
using CodeSmile.MultiPal.Settings;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.GUI
{
	[DisallowMultipleComponent]
	public sealed class GuiController : MonoBehaviour
	{
		[SerializeField] private DevMainMenu m_MainMenu;
		[SerializeField] private DevIngameMenu m_IngameMenu;

		private void Awake()
		{
			//ComponentsRegistry.Set(this);
			//ThrowIfNotAssigned<DevMainMenu>(m_MainMenu);
			//ThrowIfNotAssigned<DevIngameMenu>(m_IngameMenu);
		}

		private void Start()
		{
			CouchPlayers.OnLocalCouchPlayersSpawn += OnCouchSessionStarted;
			CouchPlayers.OnLocalCouchPlayersDespawn += OnCouchSessionStopped;

			// check if couchplayers have already spawned
			var couchPlayers = ComponentsRegistry.Get<CouchPlayers>();
			if (couchPlayers != null)
			{
				OnCouchSessionStarted(couchPlayers);

				for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
				{
					if (couchPlayers[playerIndex] != null)
						OnCouchPlayerJoined(couchPlayers, playerIndex);
				}
			}
		}

		private void OnDestroy()
		{
			CouchPlayers.OnLocalCouchPlayersSpawn -= OnCouchSessionStarted;
			CouchPlayers.OnLocalCouchPlayersDespawn -= OnCouchSessionStopped;
		}

		private void OnCouchSessionStarted(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoined += OnCouchPlayerJoined;
			couchPlayers.OnCouchPlayerLeaving += OnCouchPlayerLeaving;
		}

		private void OnCouchSessionStopped(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoined -= OnCouchPlayerJoined;
			couchPlayers.OnCouchPlayerLeaving -= OnCouchPlayerLeaving;

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
			Debug.LogWarning(GetInstanceID());
			Debug.LogWarning($"main: {m_MainMenu}");
			Debug.LogWarning($"ingame: {m_IngameMenu}");
			Debug.LogWarning($"ingame: {m_IngameMenu?.GetInstanceID()}");
			m_IngameMenu.MenuPlayerIndex = playerIndex;
			m_IngameMenu.ToggleVisible();

			var couchPlayers = ComponentsRegistry.Get<CouchPlayers>();
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
