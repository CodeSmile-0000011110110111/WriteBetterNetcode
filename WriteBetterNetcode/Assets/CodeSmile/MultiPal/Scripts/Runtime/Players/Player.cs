// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Input;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerAvatar))]
	[RequireComponent(typeof(PlayerVars), typeof(PlayerServer), typeof(PlayerClient))]
	public sealed class Player : NetworkBehaviour, IPlayerComponent
	{
		public event Action<Int32> OnSwitchCamera;
		public event Action<Int32> OnSwitchController;
		public event Action<Int32> OnRequestToggleIngameMenu;

		private PlayerAvatar m_Avatar;
		private PlayerCamera m_Camera;
		private PlayerInteraction m_Interaction;
		private PlayerClient m_ClientSide;
		private PlayerVars m_Vars;
		public PlayerCamera Camera => m_Camera;

		public Byte AvatarIndex { get => m_Vars.AvatarIndex; set => m_Vars.AvatarIndex = value; }

		public Int32 PlayerIndex { get; private set; } = -1;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticFields() {}

		public void OnPlayerSpawn(Int32 playerIndex, Boolean isOwner)
		{
			PlayerIndex = playerIndex;

			foreach (var playerComponent in GetComponentsInChildren<IPlayerComponent>())
			{
				// don't infinite recurse this
				if (Equals(playerComponent))
					continue;

				// Log component execution order (same as order on Inspector
				//Debug.Log($"OnPlayerSpawn called for: {playerComponent.GetType().Name}");

				playerComponent.OnPlayerSpawn(playerIndex, isOwner);
			}
		}

		public void OnPlayerDespawn(Int32 playerIndex, Boolean isOwner)
		{
			foreach (var playerComponent in GetComponentsInChildren<IPlayerComponent>())
			{
				// don't infinite recurse this
				if (Equals(playerComponent))
					continue;

				playerComponent.OnPlayerDespawn(playerIndex, isOwner);
			}
		}

		private void Awake()
		{
			m_Avatar = GetComponent<PlayerAvatar>();
			m_Camera = GetComponent<PlayerCamera>();
			m_Interaction = GetComponent<PlayerInteraction>();
			m_ClientSide = GetComponent<PlayerClient>();
			m_Vars = GetComponent<PlayerVars>();
		}

		public override void OnNetworkSpawn() => base.OnNetworkSpawn();

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			if (IsOwner == false)
				OnPlayerDespawn(PlayerIndex, false); // CouchPlayers can't send this, too late
		}

		public void OnOpenIngameMenu()
		{
			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.AllPlayerInteractionEnabled = false;
			inputUsers.AllPlayerKinematicsEnabled = false;
			inputUsers.AllPlayerUiEnabled = false;
			inputUsers.AllUiEnabled = true;

			// leave request menu enabled to allow for quick dismissal
			inputUsers.SetPlayerUiRequestMenuEnabled(PlayerIndex, true);
		}

		public void OnCloseIngameMenu()
		{
			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.AllPlayerInteractionEnabled = true;
			inputUsers.AllPlayerKinematicsEnabled = true;
			inputUsers.AllPlayerUiEnabled = true;
			inputUsers.AllUiEnabled = false;
		}

		internal void SwitchCamera() => OnSwitchCamera?.Invoke(PlayerIndex);
		internal void SwitchController() => OnSwitchController?.Invoke(PlayerIndex);

		internal void RequestToggleIngameMenu(Int32 playerIndex) => OnRequestToggleIngameMenu?.Invoke(playerIndex);
	}
}
