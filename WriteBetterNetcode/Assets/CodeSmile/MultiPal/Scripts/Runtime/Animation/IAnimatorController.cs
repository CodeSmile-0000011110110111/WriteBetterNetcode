// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Animation;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Interfaces
{
	public interface IAnimatorController
	{
		public AnimatorParametersBase GetAnimatorParameters(Int32 playerIndex);
		void SetAnimatorParameters(Int32 playerIndex, AnimatorParametersBase animatorParameters);
	}
}
