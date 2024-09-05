// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	public class AnimationData
	{
		public float InputMagnitude;
		public float CurrentSpeed;
		public float TargetSpeed;

		public bool IsGrounded;
		public bool IsJumping;
		public bool IsFalling;
	}

	public interface IAnimationDataProvider
	{
		public AnimationData AnimationData { get; }
	}
}
