// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine.Services.Authentication.Actions
{
	public class UnityServicesInit : FSM.IAction
	{
		public async void Execute(FSM sm)
		{
			if (UnityServices.State == ServicesInitializationState.Uninitialized)
			{
				try
				{
					await UnityServices.InitializeAsync();
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					throw;
				}
			}
		}
	}
}
