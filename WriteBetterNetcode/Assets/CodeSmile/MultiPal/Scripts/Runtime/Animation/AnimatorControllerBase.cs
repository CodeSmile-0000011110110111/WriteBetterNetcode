﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Interfaces;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	public abstract class AnimatorControllerBase : MonoBehaviour, IAnimatorController
	{
		public abstract void OnAssignAnimationData(Int32 playerIndex);
	}
}