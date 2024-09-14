// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	public abstract class GameStateConditionBase : ScriptableObject
	{
		public virtual void OnEnterState() {}
		public virtual void OnExitState() {}
		public abstract Boolean IsSatisfied();
	}
}
