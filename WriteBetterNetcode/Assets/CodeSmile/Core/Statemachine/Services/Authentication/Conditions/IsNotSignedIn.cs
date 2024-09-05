// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Core.Statemachine.Services.Authentication.Conditions
{
	public sealed class IsNotSignedIn : IsSignedIn
	{
		public override Boolean IsSatisfied(FSM sm) => !base.IsSatisfied(sm);
	}
}
