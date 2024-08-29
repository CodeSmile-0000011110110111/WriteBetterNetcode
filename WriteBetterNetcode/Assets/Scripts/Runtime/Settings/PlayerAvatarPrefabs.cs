﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Settings
{
	[CreateAssetMenu(fileName = "Player Avatars", menuName = "CodeSmile/Player Avatars", order = 0)]
	public sealed class PlayerAvatarPrefabs : ScriptableObject, IEnumerable
	{
		[SerializeField] private List<GameObject> m_Prefabs = new();
		public GameObject this[Int32 index]
		{
			get
			{
				if (index < 0 || index >= m_Prefabs.Count)
				{
					Debug.LogWarning($"avatar index {index} out of bounds!");
					return null;
				}

				return m_Prefabs[index];
			}
		}
		public Int32 Count => m_Prefabs.Count;
		public IEnumerator GetEnumerator() => m_Prefabs.GetEnumerator();
	}
}
