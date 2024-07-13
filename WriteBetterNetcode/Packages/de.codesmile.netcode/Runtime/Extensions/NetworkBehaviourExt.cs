// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode.Extensions
{
	/// <summary>
	/// Extensions for NetworkBehaviour.
	/// </summary>
	public static class NetworkBehaviourExt
	{
		/// <summary>
		/// True if we are running on a dedicated server as opposed to host.
		/// </summary>
		/// <remarks>Same as testing this: (IsServer && !IsHost)</remarks>
		/// <param name="nb"></param>
		/// <returns></returns>
		public static Boolean IsDedicatedServer(this NetworkBehaviour nb) => nb.IsServer && nb.IsHost == false;
	}
}
