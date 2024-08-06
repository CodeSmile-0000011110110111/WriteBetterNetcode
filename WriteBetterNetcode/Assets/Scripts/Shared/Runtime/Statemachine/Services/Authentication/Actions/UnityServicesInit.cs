// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Authentication.Actions
{
	public class UnityServicesInit : IAsyncAction
	{
		public Task ExecuteAsync(FSM sm)
		{
			if (UnityServices.State == ServicesInitializationState.Uninitialized)
			{
				try
				{
					return UnityServices.InitializeAsync();
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					throw;
				}
			}

			return null;
		}
	}
}
