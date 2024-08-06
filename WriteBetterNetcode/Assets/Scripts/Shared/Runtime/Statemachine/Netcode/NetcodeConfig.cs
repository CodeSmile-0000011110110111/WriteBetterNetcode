// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Netcode
{
	[Serializable]
	public struct NetcodeConfig
	{
		public NetcodeRole Role;

		public override String ToString() => $"{nameof(NetcodeConfig)}(Role={Role})";
	}
}
