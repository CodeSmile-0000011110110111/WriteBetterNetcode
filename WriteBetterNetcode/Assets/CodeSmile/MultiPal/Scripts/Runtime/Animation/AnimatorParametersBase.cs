// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	public abstract class AnimatorParametersBase
	{
		public Vector3 InputMove;
		public Vector3 InputLook;
		public bool InputJump;
		public bool InputCrouch;
		public bool InputAttack;
		public bool InputInteract;

		public Vector3 Velocity;
	}
}
