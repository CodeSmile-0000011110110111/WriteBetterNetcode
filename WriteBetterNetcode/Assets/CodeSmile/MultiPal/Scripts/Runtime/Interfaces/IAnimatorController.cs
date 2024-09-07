// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Interfaces
{
	public interface IAnimatorController
	{
		public void OnAssignAnimationData(Int32 playerIndex);
	}
}
