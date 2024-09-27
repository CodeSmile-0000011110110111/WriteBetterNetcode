// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode.Conditions
{
	/// <summary>
	///     Is true if the Netcode variable's role does not match the parameter.
	/// </summary>
	public sealed class IsNotNetcodeRole : IsNetcodeRole
	{
		/// <summary>
		///     Creates a new IsNotNetcodeRole condition.
		/// </summary>
		/// <param name="netcodeConfigVar"></param>
		/// <param name="role"></param>
		public IsNotNetcodeRole(Var<NetcodeConfig> netcodeConfigVar, NetcodeRole role)
			: base(netcodeConfigVar, role) {}

		public override Boolean IsSatisfied(FSM sm) => !base.IsSatisfied(sm);

		public override String ToDebugString(FSM sm) => $"{nameof(IsNetcodeRole)} != {m_Role}";
	}
}
