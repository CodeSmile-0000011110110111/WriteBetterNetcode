// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	/// <summary>
	///     Use this to identify a specific GameObject in a hierarchy without having to use "normal" tags or layers.
	/// </summary>
	public class StringTagComponent : TagComponent
	{
		[SerializeField] private String m_Tag;

		/// <summary>
		/// The user-supplied string tag.
		/// </summary>
		public String Tag => m_Tag;
	}
}
