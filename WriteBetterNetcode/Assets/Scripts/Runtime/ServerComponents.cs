// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	public class ServerComponents : MonoBehaviour
	{
		private static ServerComponents s_Instance;

		private void Awake()
		{
			AssignInstance();
			ThrowIfComponentIsNull();
		}

		private void AssignInstance()
		{
			if (s_Instance != null)
				throw new InvalidOperationException("already exists!");

			s_Instance = this;
		}

		private void ThrowIfComponentIsNull()
		{
			// if (ServerPlayerSpawner == null)
			// 	throw new MissingReferenceException($"{nameof(ServerPlayerSpawner)} not assigned");
		}
	}
}
