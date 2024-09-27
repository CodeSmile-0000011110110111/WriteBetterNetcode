// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	/// <summary>
	///     Use this to identify a specific GameObject in a hierarchy without having to use "normal" tags or layers.
	///     Create subclasses to be able to separated specialized tags from each other. Or use the StringTagComponent.
	/// </summary>
	public class TagComponent : MonoBehaviour {}
}
