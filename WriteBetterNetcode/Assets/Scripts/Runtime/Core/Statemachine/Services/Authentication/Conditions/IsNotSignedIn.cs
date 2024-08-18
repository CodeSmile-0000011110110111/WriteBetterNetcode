// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Core.Statemachine.Services.Authentication.Conditions
{
	public class IsNotSignedIn : IsSignedIn
	{
		public Boolean IsSatisfied(FSM sm) => !base.IsSatisfied(sm);
	}
}
