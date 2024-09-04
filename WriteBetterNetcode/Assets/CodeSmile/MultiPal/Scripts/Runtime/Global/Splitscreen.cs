// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Players;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Cameras))]
	public sealed class Splitscreen : MonoBehaviour
	{
		public enum SplitMode
		{
			Disabled,
			Horizontal,
			Vertical,
			/// <summary>
			///     Use this to create a 4-way split even when there is only one or two players.
			/// </summary>
			HorizontalAndVertical,
		}

		[Tooltip("Set to 'HorizontalAndVertical' to create a 4-way split even for just one and two players. " +
		         "3 and 4 players split will automatically 'upgrade' to a 4-way split.")]
		[SerializeField] private SplitMode m_SplitMode = SplitMode.Vertical;

		private Cameras m_Cameras;

		private void Awake() => m_Cameras = GetComponent<Cameras>();

		private void Start()
		{
			Components.OnLocalCouchPlayersSpawn += OnLocalCouchPlayersSpawn;
			Components.OnLocalCouchPlayersDespawn += OnLocalCouchPlayersDespawn;
		}

		private void OnDestroy()
		{
			Components.OnLocalCouchPlayersSpawn -= OnLocalCouchPlayersSpawn;
			Components.OnLocalCouchPlayersDespawn -= OnLocalCouchPlayersDespawn;
		}

		private void OnLocalCouchPlayersSpawn(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoin += OnCouchPlayerJoin;
			couchPlayers.OnCouchPlayerLeave += OnCouchPlayerLeave;
		}

		private void OnLocalCouchPlayersDespawn(CouchPlayers couchPlayers)
		{
			couchPlayers.OnCouchPlayerJoin -= OnCouchPlayerJoin;
			couchPlayers.OnCouchPlayerLeave -= OnCouchPlayerLeave;
		}

		internal void OnCouchPlayerJoin(CouchPlayers couchPlayers, Int32 playerIndex) => UpdateSplitscreen(couchPlayers);
		internal void OnCouchPlayerLeave(CouchPlayers couchPlayers, Int32 playerIndex) => UpdateSplitscreen(couchPlayers);

		private void UpdateSplitscreen(CouchPlayers couchPlayers)
		{
			var playerCount = couchPlayers.PlayerCount;
			if (playerCount == 1 || m_SplitMode == SplitMode.Disabled)
				EnableSinglePlayerCamera();
			else if (playerCount == 2 && m_SplitMode == SplitMode.Horizontal)
				EnableHorizontalSplitscreen();
			else if (playerCount == 2 && m_SplitMode == SplitMode.Vertical)
				EnableVerticalSplitscreen();
			// 4-way split if: requested, automatically for 3+ players
			else if (playerCount >= 3 || m_SplitMode == SplitMode.HorizontalAndVertical)
				EnableFourWaySplitscreen();
		}

		private void EnableSinglePlayerCamera()
		{
			m_Cameras.PlayerCameras[0].rect = new Rect(Vector2.zero, Vector2.one);

			SetSingleplayerCameraActive();
		}

		private void EnableHorizontalSplitscreen()
		{
			// 0 = top, 1 = bottom
			var playerCameras = m_Cameras.PlayerCameras;
			var viewportSize = new Vector2(1f, 0.5f);
			playerCameras[0].rect = new Rect(new Vector2(0f, 0.5f), viewportSize);
			playerCameras[1].rect = new Rect(Vector2.zero, viewportSize);

			SetTwoPlayerCamerasActive();
		}

		private void EnableVerticalSplitscreen()
		{
			// 0 = left, 1 = right
			var playerCameras = m_Cameras.PlayerCameras;
			var viewportSize = new Vector2(0.5f, 1f);
			playerCameras[0].rect = new Rect(Vector2.zero, viewportSize);
			playerCameras[1].rect = new Rect(new Vector2(0.5f, 0f), viewportSize);

			SetTwoPlayerCamerasActive();
		}

		private void EnableFourWaySplitscreen()
		{
			// 0 = top left, 1 = top right, 2 = bottom left, 3 = bottom right
			var playerCameras = m_Cameras.PlayerCameras;
			var viewportSize = new Vector2(0.5f, 0.5f);
			playerCameras[0].rect = new Rect(new Vector2(0f, 0.5f), viewportSize);
			playerCameras[1].rect = new Rect(new Vector2(0.5f, 0.5f), viewportSize);
			playerCameras[2].rect = new Rect(new Vector2(0f, 0f), viewportSize);
			playerCameras[3].rect = new Rect(new Vector2(0.5f, 0f), viewportSize);

			SetAllCamerasActive();
		}

		private void SetSingleplayerCameraActive()
		{
			var playerCameras = m_Cameras.PlayerCameras;
			for (var i = 0; i < playerCameras.Length; i++)
				playerCameras[i].gameObject.SetActive(i == 0);
		}

		private void SetTwoPlayerCamerasActive()
		{
			var playerCameras = m_Cameras.PlayerCameras;
			for (var i = 0; i < playerCameras.Length; i++)
				playerCameras[i].gameObject.SetActive(i == 0 || i == 1);
		}

		private void SetAllCamerasActive()
		{
			var playerCameras = m_Cameras.PlayerCameras;
			foreach (var playerCamera in playerCameras)
				playerCamera.gameObject.SetActive(true);
		}
	}
}
