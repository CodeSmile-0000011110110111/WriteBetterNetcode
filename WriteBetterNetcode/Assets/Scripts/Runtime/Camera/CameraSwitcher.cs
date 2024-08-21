// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	[DisallowMultipleComponent]
	public sealed class CameraSwitcher : MonoBehaviour
	{
		[SerializeField] private Camera m_OfflineCamera;
		[SerializeField] private Camera m_OnlineCamera;

		private void Start()
		{
			var netcodeState = Components.NetcodeState;
			netcodeState.WentOnline += WentOnline;
			netcodeState.WentOffline += WentOffline;
		}

		private void OnDestroy()
		{
			var netcodeState = Components.NetcodeState;
			netcodeState.WentOnline -= WentOnline;
			netcodeState.WentOffline -= WentOffline;
		}

		private void WentOnline()
		{
			Debug.Log("Camera: went online");
			m_OnlineCamera.gameObject.SetActive(true);
			m_OfflineCamera.gameObject.SetActive(false);
		}

		private void WentOffline()
		{
			Debug.Log("Camera: went offline");
			m_OnlineCamera.gameObject.SetActive(false);
			m_OfflineCamera.gameObject.SetActive(true);
		}
	}
}
