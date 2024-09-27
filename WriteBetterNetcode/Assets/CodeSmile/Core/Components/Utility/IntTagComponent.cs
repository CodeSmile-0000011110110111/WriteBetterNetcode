// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	/// <summary>
	///     Use this to identify a specific GameObject in a hierarchy by integer, without having to use "normal" tags or
	///     layers.
	/// </summary>
	public class IntTagComponent : TagComponent
	{
		[SerializeField] private Int32 m_Tag;

		/// <summary>
		///     The integer tag.
		/// </summary>
		public Int32 Tag => m_Tag;
	}
}
