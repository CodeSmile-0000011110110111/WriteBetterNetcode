// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEngine;

namespace CodeSmile.Components
{
	/// <summary>
	/// Marks the GameObject this script is on as "Don't Destroy On Load".
	/// </summary>
	/// <remarks>
	/// DDoL is applied in Start(), not Awake(), to allow Multiplayer Roles to strip components during Awake().
	/// This script will also move the GameObject to the root since DDoL only works on root game objects.
	/// </remarks>
	public class DontDestroyOnLoad : MonoBehaviour
	{
		[SerializeField] private bool m_SetInactiveOnLoad;

		private void Start()
		{
			if (enabled)
			{
				Apply(gameObject);

				if (m_SetInactiveOnLoad)
					gameObject.SetActive(false);

				Destroy(this);
			}
		}

		/// <summary>
		/// Applies "Don't Destroy On Load" to the GameObject and will also move it to the scene root since
		/// this is required by DDoL.
		/// </summary>
		/// <param name="go"></param>
		public static void Apply(GameObject go)
		{
			// DDoL only works on root game objects
			go.transform.parent = null;

			DontDestroyOnLoad(go);
		}
	}
}
