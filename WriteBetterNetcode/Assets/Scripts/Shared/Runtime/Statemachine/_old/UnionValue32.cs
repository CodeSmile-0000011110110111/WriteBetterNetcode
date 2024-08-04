// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct UnionValue32
	{
		[FieldOffset(0)] public Boolean BoolValue;
		[FieldOffset(0)] public Single FloatValue;
		[FieldOffset(0)] public Int32 IntValue;
	}
}
