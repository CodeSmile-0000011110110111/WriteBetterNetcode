// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	/// <summary>
	///     Hooks the Escape key to Application.Quit.
	///     <remarks>
	///         It annoys me that builds no longer support Alt+F4 and thus there is no way
	///         to quit a build except going through Task Manager.
	///     </remarks>
	/// </summary>
	internal class PressKeyToQuit : MonoBehaviour
	{
		[Tooltip("The key that will quit if Ctrl+Alt / Cmd+Option are also held down.")]
		public KeyCode QuitKey = KeyCode.Escape;

		private void Awake()
		{
#if UNITY_EDITOR || !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX)
			Destroy(gameObject);
#endif
		}

		private void Update()
		{
			var alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
			if (!alt)
				return;

			var cmd = Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
			var ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			if (!cmd && !ctrl)
				return;

			if (Input.GetKeyDown(QuitKey))
				Application.Quit();
		}
	}
}
