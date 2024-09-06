// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Utility;
using CodeSmile.MultiPal.Input;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.MultiPal.Player;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Global
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ComponentsRegistry))]
	public class Components : MonoBehaviour, IComponents
	{
		public static event Action<CouchPlayers> OnLocalCouchPlayersSpawn;
		public static event Action<CouchPlayers> OnLocalCouchPlayersDespawn;

		[SerializeField] private NetcodeState m_NetcodeState;
		[SerializeField] private InputUsers m_InputUsers;
		[SerializeField] private Cameras m_Cameras;
		[SerializeField] private PlayerControllers m_PlayerControllers;
		[SerializeField] private CouchPlayers m_CouchPlayers;

		private static void ResetStaticFields()
		{
			OnLocalCouchPlayersSpawn = null;
			OnLocalCouchPlayersDespawn = null;
		}

		public T Get<T>() where T : MonoBehaviour
		{
			switch (typeof(T).Name)
			{
				case nameof(NetcodeState):
					return m_NetcodeState as T;
				case nameof(InputUsers):
					return m_InputUsers as T;
				case nameof(Cameras):
					return m_Cameras as T;
				case nameof(PlayerControllers):
					return m_PlayerControllers as T;
				case nameof(CouchPlayers):
					return m_CouchPlayers as T;

				default:
					throw new ArgumentOutOfRangeException(nameof(T), "unhandled type");
			}
		}

		public void Set<T>(T component) where T : MonoBehaviour
		{
			switch (typeof(T).Name)
			{
				case nameof(CouchPlayers):
					SetLocalCouchPlayers(component as CouchPlayers);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(T), "unhandled or read-only type");
			}
		}

		private void Awake()
		{
			ThrowIfNotAssigned<NetcodeState>(m_NetcodeState);
			ThrowIfNotAssigned<InputUsers>(m_InputUsers);
			ThrowIfNotAssigned<Cameras>(m_Cameras);
			ThrowIfNotAssigned<PlayerControllers>(m_PlayerControllers);
		}

		private void OnDestroy() => ResetStaticFields();

		private void ThrowIfNotAssigned<T>(Component component) where T : Component
		{
			if (component == null || component is not T)
				throw new MissingReferenceException($"{typeof(T).Name} not assigned");
		}

		private void SetLocalCouchPlayers(CouchPlayers couchPlayers)
		{
			var isCouchPlayersNull = couchPlayers == null;
			if (m_CouchPlayers != null && isCouchPlayersNull == false)
				throw new ArgumentException("local couch players already assigned, replace not allowed; possible bug?");

			if (isCouchPlayersNull)
				OnLocalCouchPlayersDespawn?.Invoke(m_CouchPlayers);

			m_CouchPlayers = couchPlayers;

			if (isCouchPlayersNull == false)
				OnLocalCouchPlayersSpawn?.Invoke(m_CouchPlayers);
		}
	}
}
