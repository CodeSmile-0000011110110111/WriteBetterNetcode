// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	public sealed partial class FSM
	{
		public interface IAction : IStatemachineEvents
		{
			void Execute(FSM sm);
			String ToDebugString(FSM sm) => GetType().Name;
		}
	}
}
