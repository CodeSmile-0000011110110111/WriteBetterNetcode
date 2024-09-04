// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Players;
using CodeSmile.Settings;
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

		internal void UpdateSplitscreen(CouchPlayers couchPlayers)
		{
			var twoPlayers = AreTwoPlayersPlaying(couchPlayers);
			var playerCount = couchPlayers.PlayerCount;
			if (playerCount <= 1 || m_SplitMode == SplitMode.Disabled)
				EnableSinglePlayerCamera();
			else if (twoPlayers && m_SplitMode == SplitMode.Horizontal)
				EnableHorizontalSplitscreen();
			else if (twoPlayers && m_SplitMode == SplitMode.Vertical)
				EnableVerticalSplitscreen();
			// 4-way split if: requested, automatically for 3+ players
			else if (playerCount >= 3 || m_SplitMode == SplitMode.HorizontalAndVertical)
				EnableFourWaySplitscreen();
		}

		/// <summary>
		///     Check if the first two players are playing. We may have 2 players playing but these may be players 0 and 3,
		///     indicating a previous four-way splitscreen mode and thus we should keep the four-way split.
		/// </summary>
		/// <param name="couchPlayers"></param>
		/// <returns></returns>
		private Boolean AreTwoPlayersPlaying(CouchPlayers couchPlayers) =>
			couchPlayers.PlayerCount == 2 && couchPlayers[0] != null && couchPlayers[1] != null;

		private void EnableSinglePlayerCamera()
		{
			Debug.Log("singleplayer");
			SetSingleplayerCameraActive();

			m_Cameras.PlayerCameras[0].rect = new Rect(Vector2.zero, Vector2.one);
		}

		private void EnableHorizontalSplitscreen()
		{
			Debug.Log("two players");
			SetTwoPlayerCamerasActive();

			// 0 = top, 1 = bottom
			var playerCameras = m_Cameras.PlayerCameras;
			var viewportSize = new Vector2(1f, 0.5f);
			playerCameras[0].rect = new Rect(new Vector2(0f, 0.5f), viewportSize);
			playerCameras[1].rect = new Rect(Vector2.zero, viewportSize);
		}

		private void EnableVerticalSplitscreen()
		{
			Debug.Log("two players");
			SetTwoPlayerCamerasActive();

			// 0 = left, 1 = right
			var playerCameras = m_Cameras.PlayerCameras;
			var viewportSize = new Vector2(0.5f, 1f);
			playerCameras[0].rect = new Rect(Vector2.zero, viewportSize);
			playerCameras[1].rect = new Rect(new Vector2(0.5f, 0f), viewportSize);
		}

		private void EnableFourWaySplitscreen()
		{
			Debug.Log("3-4 players");
			SetAllPlayerCamerasActive();

			// 0 = top left, 1 = top right, 2 = bottom left, 3 = bottom right
			var cameras = m_Cameras.PlayerCameras;
			var viewportSize = new Vector2(0.5f, 0.5f);
			cameras[0].rect = new Rect(new Vector2(0f, 0.5f), viewportSize);
			cameras[1].rect = new Rect(new Vector2(0.5f, 0.5f), viewportSize);
			cameras[2].rect = new Rect(new Vector2(0f, 0f), viewportSize);
			cameras[3].rect = new Rect(new Vector2(0.5f, 0f), viewportSize);
		}

		private void SetSingleplayerCameraActive()
		{
			var playerCameras = m_Cameras.PlayerCameras;
			for (var i = 0; i < Constants.MaxCouchPlayers; i++)
				playerCameras[i].gameObject.SetActive(i == 0);
		}

		private void SetTwoPlayerCamerasActive()
		{
			var playerCameras = m_Cameras.PlayerCameras;
			for (var i = 0; i < Constants.MaxCouchPlayers; i++)
				playerCameras[i].gameObject.SetActive(i == 0 || i == 1);
		}

		private void SetAllPlayerCamerasActive()
		{
			var playerCameras = m_Cameras.PlayerCameras;
			for (var i = 0; i < Constants.MaxCouchPlayers; i++)
				playerCameras[i].gameObject.SetActive(true);
		}
	}
}
