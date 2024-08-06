// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Authentication.Actions
{
	public class SignInAnonymously : FSM.IAsyncAction
	{
		public Task ExecuteAsync(FSM sm)
		{
			var authService = AuthenticationService.Instance;
			if (authService.IsSignedIn == false)
			{
				try
				{
					return authService.SignInAnonymouslyAsync();
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					//throw;
				}
			}

			return null;
		}
	}
}
