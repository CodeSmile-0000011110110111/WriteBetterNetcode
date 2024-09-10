// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	public interface IAnimatorController
	{
		void Init(Int32 playerIndex, Boolean isOwner);
		void RemoteAnimatorParametersReceived(Byte[] animatorParameters);
	}
}
