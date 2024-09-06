// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	public interface IComponents
	{
		public T Get<T>() where T : MonoBehaviour;
		public void Set<T>(T component) where T : MonoBehaviour;
	}
}
