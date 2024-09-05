// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Core.Statemachine.Services.Authentication.Actions
{
	public sealed class SignInAnonymously : IAsyncAction
	{
		public async Task ExecuteAsync(FSM sm)
		{
			// Intentional: if services not initialized, exception is thrown

			var authService = AuthenticationService.Instance;
			if (authService.IsSignedIn == false)
				await authService.SignInAnonymouslyAsync();
		}
	}
}
