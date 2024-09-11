// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.GUI
{
	[DisallowMultipleComponent]
	public sealed class PregameGuiController : MonoBehaviour
	{
		[SerializeField] private DevMainMenu m_MainMenu;

		private void Awake() => ThrowIfNotAssigned<DevMainMenu>(m_MainMenu);

		private void ThrowIfNotAssigned<T>(Component component) where T : Component
		{
			if (component == null || component is not T)
				throw new MissingReferenceException($"{typeof(T).Name} not assigned");
		}
	}
}
