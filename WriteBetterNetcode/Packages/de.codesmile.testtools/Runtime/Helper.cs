// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using NUnit.Framework;
using System;
using UnityEngine;

namespace CodeSmile.TestTools
{
	public static class Helper
	{
		public static GameObject CreateGameObjectWithChildren(Int32 childCount)
		{
			var parent = new GameObject("parent");
			for (var i = 0; i < childCount; i++)
			{
				var child = new GameObject("child");
				child.transform.parent = parent.transform;
			}

			Assert.AreEqual(childCount, parent.transform.childCount);

			return parent;
		}
	}
}
