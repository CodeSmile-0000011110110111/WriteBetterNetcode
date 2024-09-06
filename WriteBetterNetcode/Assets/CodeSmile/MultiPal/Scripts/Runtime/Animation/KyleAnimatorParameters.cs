// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	public class KyleAnimatorParameters : AnimatorParametersBase
	{
		public Single CurrentSpeed;
		public Single TargetSpeed;

		public Boolean IsGrounded;
		public Boolean IsFalling;

		public void SetKinematicParams(Single inputMagnitude, Single velocityMagnitude, Boolean charControllerIsGrounded,
			Boolean didJump)
		{
			CurrentSpeed = TargetSpeed = velocityMagnitude;
			IsGrounded = charControllerIsGrounded;
			IsFalling = !charControllerIsGrounded;
		}
	}
}
