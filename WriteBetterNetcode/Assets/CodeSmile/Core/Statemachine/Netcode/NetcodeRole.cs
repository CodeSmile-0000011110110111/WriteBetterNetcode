// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	/// <summary>
	///     The role we want to play as, with None signaling undecided or intention to stop playing in the current role.
	/// </summary>
	public enum NetcodeRole
	{
		None,
		Server,
		Host,
		Client,
	}
}
