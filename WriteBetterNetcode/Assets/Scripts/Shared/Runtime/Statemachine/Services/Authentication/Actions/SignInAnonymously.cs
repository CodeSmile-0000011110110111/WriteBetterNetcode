// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Authentication.Actions
{
	public class SignInAnonymously : IAsyncAction
	{
		public Task ExecuteAsync(FSM sm)
		{
			var authService = AuthenticationService.Instance;
			if (authService.IsSignedIn == false)
				return authService.SignInAnonymouslyAsync();

			return null;
		}
	}
}
