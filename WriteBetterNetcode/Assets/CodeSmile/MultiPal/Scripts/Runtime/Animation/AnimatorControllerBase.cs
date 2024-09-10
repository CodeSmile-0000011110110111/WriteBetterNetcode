﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	public abstract class AnimatorControllerBase : MonoBehaviour, IAnimatorController
	{
		public Boolean IsOwner { get; protected set; }
		public Int32 PlayerIndex { get; protected set; }

		public abstract void Init(Int32 playerIndex, Boolean isOwner);

		public abstract void RemoteAnimatorParametersReceived(Byte[] animatorParameters);
	}
}
