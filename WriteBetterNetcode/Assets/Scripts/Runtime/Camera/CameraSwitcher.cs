// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Settings;
using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	[DisallowMultipleComponent]
	public sealed class CameraSwitcher : MonoBehaviour
	{
		[SerializeField] private Camera m_OfflineCamera;
		[SerializeField] private Camera m_OnlineCamera;
		[SerializeField] private Camera[] m_SplitscreenPlayerCameras = new Camera[Constants.MaxCouchPlayers];

		private void Start()
		{
			var netcodeState = Components.NetcodeState;
			netcodeState.WentOnline += WentOnline;
			netcodeState.WentOffline += WentOffline;
		}

		private void OnDestroy()
		{
			var netcodeState = Components.NetcodeState;
			if (netcodeState != null)
			{
				netcodeState.WentOnline -= WentOnline;
				netcodeState.WentOffline -= WentOffline;
			}
		}

		private void WentOnline()
		{
			m_OnlineCamera.gameObject.SetActive(true);
			m_OfflineCamera.gameObject.SetActive(false);
		}

		private void WentOffline()
		{
			m_OnlineCamera.gameObject.SetActive(false);
			m_OfflineCamera.gameObject.SetActive(true);
		}
	}
}
