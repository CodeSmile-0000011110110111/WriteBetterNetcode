// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Input;
using System;
using System.Linq;
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
		public event Action<Player> OnDeath;
		public event Action<Player> OnRespawn;

		private PlayerAvatar m_Avatar;
		private PlayerCamera m_Camera;
		private PlayerInteraction m_Interaction;
		private PlayerClient m_ClientSide;
		private PlayerVars m_Vars;
		private IPlayerComponent[] m_PlayerComponents;
		public PlayerCamera Camera => m_Camera;

		public Byte AvatarIndex { get => m_Vars.AvatarIndex; set => m_Vars.AvatarIndex = value; }

		public Int32 PlayerIndex { get; private set; } = -1;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticFields() {}

		public void OnPlayerSpawn(Int32 playerIndex, Boolean isOwner)
		{
			PlayerIndex = playerIndex;

			GetPlayerComponentsExceptThis();

			foreach (var playerComponent in m_PlayerComponents)
				// Log component execution order (same as order on Inspector
				//Debug.Log($"OnPlayerSpawn called for: {playerComponent.GetType().Name}");
				playerComponent.OnPlayerSpawn(playerIndex, isOwner);
		}

		public void OnPlayerDespawn(Int32 playerIndex, Boolean isOwner)
		{
			foreach (var playerComponent in m_PlayerComponents)
				playerComponent.OnPlayerDespawn(playerIndex, isOwner);
		}

		private void Awake()
		{
			m_Avatar = GetComponent<PlayerAvatar>();
			m_Camera = GetComponent<PlayerCamera>();
			m_Interaction = GetComponent<PlayerInteraction>();
			m_ClientSide = GetComponent<PlayerClient>();
			m_Vars = GetComponent<PlayerVars>();
		}

		private void GetPlayerComponentsExceptThis()
		{
			var playerComponents = GetComponentsInChildren<IPlayerComponent>(true).ToList();
			playerComponents.Remove(this); // avoid infinite recursions
			m_PlayerComponents = playerComponents.ToArray();
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

		private bool m_IsDead;
		internal void Die()
		{
			if (m_IsDead)
				return;

			m_IsDead = true;

			foreach (var playerComponent in m_PlayerComponents)
				playerComponent.OnPlayerDeath(PlayerIndex, IsOwner);

			OnDeath?.Invoke(this);
		}

		internal void Respawn()
		{
			if (m_IsDead == false)
				return;

			m_IsDead = false;

			foreach (var playerComponent in m_PlayerComponents)
				playerComponent.OnPlayerRespawn(PlayerIndex, IsOwner);

			OnRespawn?.Invoke(this);
		}
	}
}
