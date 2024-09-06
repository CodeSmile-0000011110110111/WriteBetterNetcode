// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Input;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.MultiPal.Player;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Global
{
	public class ComponentsRegistry : MonoBehaviour
	{
		public static event Action<CouchPlayers> OnLocalCouchPlayersSpawn;
		public static event Action<CouchPlayers> OnLocalCouchPlayersDespawn;

		private static ComponentsRegistry s_Instance;

		[SerializeField] private NetcodeState m_NetcodeState;
		[SerializeField] private InputUsers m_InputUsers;
		[SerializeField] private Cameras m_Cameras;
		[SerializeField] private PlayerControllers m_PlayerControllers;
		[SerializeField] private CouchPlayers m_LocalCouchPlayers;

		public static NetcodeState NetcodeState => s_Instance?.m_NetcodeState;
		public static InputUsers InputUsers => s_Instance?.m_InputUsers;
		public static Cameras Cameras => s_Instance?.m_Cameras;
		public static PlayerControllers PlayerControllers => s_Instance?.m_PlayerControllers;
		public static CouchPlayers LocalCouchPlayers
		{
			get => s_Instance?.m_LocalCouchPlayers;
			set
			{
				if (s_Instance?.m_LocalCouchPlayers != null && value != null)
					throw new ArgumentException("local couch players already assigned, replace not allowed; possible bug?");

				if (value == null)
					OnLocalCouchPlayersDespawn?.Invoke(s_Instance.m_LocalCouchPlayers);

				s_Instance.m_LocalCouchPlayers = value;

				if (value != null)
					OnLocalCouchPlayersSpawn?.Invoke(value);
			}
		}

		private static void ResetStaticFields()
		{
			s_Instance = null;
			OnLocalCouchPlayersSpawn = null;
			OnLocalCouchPlayersDespawn = null;
		}

		private void Awake()
		{
			AssignInstance();

			ThrowIfNotAssigned<NetcodeState>(m_NetcodeState);
			ThrowIfNotAssigned<InputUsers>(m_InputUsers);
			ThrowIfNotAssigned<Cameras>(m_Cameras);
			ThrowIfNotAssigned<PlayerControllers>(m_PlayerControllers);
		}

		private void OnDestroy() => ResetStaticFields();

		private void AssignInstance()
		{
			if (s_Instance != null)
				throw new InvalidOperationException("already exists!");

			s_Instance = this;
		}

		private void ThrowIfNotAssigned<T>(Component component) where T : Component
		{
			if (component == null || component is not T)
				throw new MissingReferenceException($"{typeof(T).Name} not assigned");
		}
	}
}
