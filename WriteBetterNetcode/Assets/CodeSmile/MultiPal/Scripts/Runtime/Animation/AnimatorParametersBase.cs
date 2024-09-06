// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	public abstract class AnimatorParametersBase
	{
		public Vector3 InputMove;
		public Vector3 InputLook;
		public Boolean InputJump;
		public Boolean InputCrouch;
		public Boolean InputAttack;
		public Boolean InputInteract;

		public Vector3 Velocity;
	}
}
