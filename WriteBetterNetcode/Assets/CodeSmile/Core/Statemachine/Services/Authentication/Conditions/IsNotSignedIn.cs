// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Authentication.Conditions
{
	/// <summary>
	///     Is true if user is not signed in (not authenticated) or services are not initialized.
	/// </summary>
	public sealed class IsNotSignedIn : IsSignedIn
	{
		public override Boolean IsSatisfied(FSM sm) => !base.IsSatisfied(sm);
	}
}
