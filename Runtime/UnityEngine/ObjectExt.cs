﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeSmile
{
	/// <summary>
	///     UnityEngine.Object extension methods
	/// </summary>
	public static class ObjectExt
	{
		/// <summary>
		///     Destroys the object regardless of Edit or Play mode by calling either
		///		DestroyImmediate or Destroy depending on Application.isPlaying.
		/// </summary>
		/// <remarks>
		///     In Builds it compiles to a direct, inlined call to Object.Destroy() so there
		///     is no condition test and no loss of performance.
		/// </remarks>
		/// <param name="self"></param>
#if UNITY_EDITOR
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DestroyInAnyMode(this Object self)
		{
			if (Application.isPlaying == false)
				Object.DestroyImmediate(self);
			else
				Object.Destroy(self);
		}
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DestroyInAnyMode(this Object self) => Object.Destroy(self);
#endif
	}
}
