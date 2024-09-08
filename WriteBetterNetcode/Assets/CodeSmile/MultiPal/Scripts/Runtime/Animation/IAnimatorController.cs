// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	public interface IAnimatorController
	{
		public AvatarAnimatorParameters GetAnimatorParameters(Int32 playerIndex);
		void SetAnimatorParameters(Int32 playerIndex, AvatarAnimatorParameters avatarAnimatorParameters);
	}
}
