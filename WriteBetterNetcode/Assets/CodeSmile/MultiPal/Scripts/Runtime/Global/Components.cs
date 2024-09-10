// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Cinecam;
using CodeSmile.MultiPal.Input;
using CodeSmile.MultiPal.Netcode;
using CodeSmile.MultiPal.PlayerController;
using CodeSmile.MultiPal.Players.Couch;
using CodeSmile.MultiPal.Scene;
using System;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.MultiPal.Global
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ComponentsRegistry))]
	public class Components : MonoBehaviour, IComponents
	{
		[SerializeField] private NetcodeState m_NetcodeState;
		[SerializeField] private InputUsers m_InputUsers;
		[SerializeField] private Cameras m_Cameras;
		[SerializeField] private PlayerControllers m_PlayerControllers;
		[SerializeField] private CouchPlayers m_CouchPlayers;
		[SerializeField] private SceneLoader m_SceneLoader;

		public T Get<T>() where T : Component
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
				case nameof(SceneLoader):
					return m_SceneLoader as T;

				default:
					throw new ArgumentOutOfRangeException(nameof(T), "unhandled type");
			}
		}

		public void Set<T>(T component) where T : Component
		{
			switch (typeof(T).Name)
			{
				case nameof(CouchPlayers):
					ThrowIfAlreadyAssigned(m_CouchPlayers, component);
					m_CouchPlayers = component as CouchPlayers;
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
			ThrowIfNotAssigned<SceneLoader>(m_SceneLoader);
		}

		private void ThrowIfNotAssigned<T>(Component component) where T : Component
		{
			if (component == null || component is not T)
				throw new MissingReferenceException($"{typeof(T).Name} not assigned");
		}

		private void ThrowIfAlreadyAssigned(Object field, Component component)
		{
			if (field != null && component != null)
				throw new ArgumentException($"{component.GetType().Name} already assigned, replace not allowed; possible bug?");
		}
	}
}
