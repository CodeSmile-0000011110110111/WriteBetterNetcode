// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Core.Actions
{
	/// <summary>
	/// Initialize Unity Gaming Services. This must be called before any other service call.
	/// </summary>
	public sealed class UnityServicesInit : IAsyncAction
	{
		public async Task ExecuteAsync(FSM sm)
		{
			if (UnityServices.State == ServicesInitializationState.Uninitialized)
				await UnityServices.InitializeAsync();
		}
	}
}
