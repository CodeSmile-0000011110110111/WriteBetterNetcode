// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	public static class StringExt
	{
		public static String RemoveWhitespace(this String s) => new(s.Where(c => !Char.IsWhiteSpace(c)).ToArray());
	}
}
