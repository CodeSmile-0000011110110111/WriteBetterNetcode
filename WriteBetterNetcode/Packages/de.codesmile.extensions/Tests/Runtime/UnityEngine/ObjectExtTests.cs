// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CodeSmile.Tests
{
	public class ObjectExtTests
	{
		[UnityTest]
		public IEnumerator DestroyInAnyMode_DestroysTheGameObject()
		{
			var parent = Helper.CreateGameObjectWithChildren(0);

			parent.transform.DestroyInAnyMode();
			yield return null;

			var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			foreach (var go in rootObjects)
				Assert.False(go.name.Equals("parent"));
		}
	}
}
