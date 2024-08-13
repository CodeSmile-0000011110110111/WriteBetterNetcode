// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Core.Actions
{
	public class UnityServicesInit : IAsyncAction
	{
		public Task ExecuteAsync(FSM sm)
		{
			if (UnityServices.State == ServicesInitializationState.Uninitialized)
				return UnityServices.InitializeAsync();

			return null;
		}
	}
}
