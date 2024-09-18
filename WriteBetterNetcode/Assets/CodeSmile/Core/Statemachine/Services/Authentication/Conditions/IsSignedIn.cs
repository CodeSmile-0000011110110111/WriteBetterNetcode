// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Authentication.Conditions
{
	/// <summary>
	/// Is true if services are initialized and user is signed in.
	/// </summary>
	public class IsSignedIn : ICondition
	{
		public virtual Boolean IsSatisfied(FSM sm) => UnityServices.State == ServicesInitializationState.Initialized &&
		                                              AuthenticationService.Instance.IsSignedIn;
	}
}
