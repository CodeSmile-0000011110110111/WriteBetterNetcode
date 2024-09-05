// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Players;
using CodeSmile.MultiPal.Settings;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal
{
	/// <summary>
	///     Dynamically splits the screen based on number of currently joined players.
	///     Splitscreen behaviour is as follows:
	///     1 player = 1 camera active
	///     2 players = 2 cameras active (horizontal or vertical split)
	///     3-4 players = 4 cameras active
	///     In 4-way splitscreen, if player 3 or 4 remain in the game, the screen remains 4-way split.
	///     If Axis is set to HorizontalAndVertical the split will be 4-way regardless of player count.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class Splitscreen : MonoBehaviour
	{
		public enum SplitscreenAxis
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
		[SerializeField] private SplitscreenAxis m_SplitscreenAxis = SplitscreenAxis.Vertical;

		private Cameras m_Cameras;
		public SplitscreenAxis SplitAxis
		{
			get => m_SplitscreenAxis;
			set
			{
				if (m_SplitscreenAxis != value)
				{
					m_SplitscreenAxis = value;

					var couchPlayers = Components.LocalCouchPlayers;
					if (couchPlayers != null)
						UpdateSplitscreen(couchPlayers);
				}
			}
		}

		private void Awake() => m_Cameras = GetComponent<Cameras>();

		internal void UpdateSplitscreen(CouchPlayers couchPlayers)
		{
			var firstTwoPlayers = AreOnlyFirstTwoPlayersPlaying(couchPlayers);
			var playerCount = couchPlayers.PlayerCount;
			if ((playerCount <= 1 || m_SplitscreenAxis == SplitscreenAxis.Disabled) &&
			    m_SplitscreenAxis != SplitscreenAxis.HorizontalAndVertical)
				EnableSinglePlayerCamera();
			else if (firstTwoPlayers && m_SplitscreenAxis == SplitscreenAxis.Horizontal)
				EnableHorizontalSplitscreen();
			else if (firstTwoPlayers && m_SplitscreenAxis == SplitscreenAxis.Vertical)
				EnableVerticalSplitscreen();
			else // 4-way split if set to both H/V, automatically for 3+ players, or if players 3 or 4 remain ingame
				EnableFourWaySplitscreen();
		}

		/// <summary>
		///     Check if the first two players are playing. We may have 2 players playing but these may be players 0 and 3,
		///     indicating a previous four-way splitscreen mode and thus we should keep the four-way split.
		/// </summary>
		/// <param name="couchPlayers"></param>
		/// <returns></returns>
		private Boolean AreOnlyFirstTwoPlayersPlaying(CouchPlayers couchPlayers) =>
			couchPlayers.PlayerCount == 2 && couchPlayers[0] != null && couchPlayers[1] != null;

		private void EnableSinglePlayerCamera()
		{
			Debug.Log("singleplayer");
			SetSingleplayerCameraActive();

			m_Cameras.PlayerSplitCameras[0].rect = new Rect(Vector2.zero, Vector2.one);
		}

		private void EnableHorizontalSplitscreen()
		{
			Debug.Log("two players");
			SetTwoPlayerCamerasActive();

			// 0 = top, 1 = bottom
			var playerCameras = m_Cameras.PlayerSplitCameras;
			var viewportSize = new Vector2(1f, 0.5f);
			playerCameras[0].rect = new Rect(new Vector2(0f, 0.5f), viewportSize);
			playerCameras[1].rect = new Rect(Vector2.zero, viewportSize);
		}

		private void EnableVerticalSplitscreen()
		{
			Debug.Log("two players");
			SetTwoPlayerCamerasActive();

			// 0 = left, 1 = right
			var playerCameras = m_Cameras.PlayerSplitCameras;
			var viewportSize = new Vector2(0.5f, 1f);
			playerCameras[0].rect = new Rect(Vector2.zero, viewportSize);
			playerCameras[1].rect = new Rect(new Vector2(0.5f, 0f), viewportSize);
		}

		private void EnableFourWaySplitscreen()
		{
			Debug.Log("3-4 players");
			SetAllPlayerCamerasActive();

			// 0 = top left, 1 = top right, 2 = bottom left, 3 = bottom right
			var cameras = m_Cameras.PlayerSplitCameras;
			var viewportSize = new Vector2(0.5f, 0.5f);
			cameras[0].rect = new Rect(new Vector2(0f, 0.5f), viewportSize);
			cameras[1].rect = new Rect(new Vector2(0.5f, 0.5f), viewportSize);
			cameras[2].rect = new Rect(new Vector2(0f, 0f), viewportSize);
			cameras[3].rect = new Rect(new Vector2(0.5f, 0f), viewportSize);
		}

		private void SetSingleplayerCameraActive()
		{
			var playerCameras = m_Cameras.PlayerSplitCameras;
			var notJoinedCameras = m_Cameras.PlayerNotJoinedCinecams;
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				playerCameras[playerIndex].gameObject.SetActive(playerIndex == 0);
				notJoinedCameras[playerIndex].gameObject.SetActive(false);
			}
		}

		private void SetTwoPlayerCamerasActive()
		{
			var playerCameras = m_Cameras.PlayerSplitCameras;
			var notJoinedCameras = m_Cameras.PlayerNotJoinedCinecams;
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				playerCameras[playerIndex].gameObject.SetActive(playerIndex == 0 || playerIndex == 1);
				notJoinedCameras[playerIndex].gameObject.SetActive(false);
			}
		}

		private void SetAllPlayerCamerasActive()
		{
			var couchPlayers = Components.LocalCouchPlayers;
			var playerCameras = m_Cameras.PlayerSplitCameras;
			var notJoinedCameras = m_Cameras.PlayerNotJoinedCinecams;
			for (var playerIndex = 0; playerIndex < Constants.MaxCouchPlayers; playerIndex++)
			{
				playerCameras[playerIndex].gameObject.SetActive(true);

				var isPlaying = couchPlayers[playerIndex] != null;
				notJoinedCameras[playerIndex].gameObject.SetActive(!isPlaying);
			}
		}
	}
}
