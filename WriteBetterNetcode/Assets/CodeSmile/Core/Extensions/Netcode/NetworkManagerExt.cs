// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace CodeSmile.Extensions.Netcode
{
	/// <summary>
	///     Extension and utility methods for NetworkManager
	/// </summary>
	public static class NetworkManagerExt
	{
		/// <summary>
		///     Shorthand for calling: NetworkManager.Singleton.GetComponent&lt;UnityTransport&gt;();
		/// </summary>
		/// <returns></returns>
		/// <returns>The UnityTransport component.</returns>
		public static UnityTransport GetTransport(this NetworkManager netMan) => netMan.GetComponent<UnityTransport>();
	}
}
