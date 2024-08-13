// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Services.Authentication;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Authentication.Conditions
{
	public class IsNotSignedIn : ICondition
	{
		public Boolean IsSatisfied(FSM sm) => !AuthenticationService.Instance.IsSignedIn;
	}
}
