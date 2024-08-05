// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Services.Authentication;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Authentication.Actions
{
	public class SignInAnonymously : FSM.IAction
	{
		public async void Execute(FSM sm)
		{
			var authService = AuthenticationService.Instance;
			if (authService.IsSignedIn)
				return;

			try
			{
				await authService.SignInAnonymouslyAsync();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				throw;
			}
		}
	}
}
