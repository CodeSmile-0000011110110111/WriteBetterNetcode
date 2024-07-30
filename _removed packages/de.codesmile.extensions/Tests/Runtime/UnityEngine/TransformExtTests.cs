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
	public class TransformExtTests
	{
		[UnityTest]
		public IEnumerator DestroyAllChildren_NoChildren_DoesNotThrow()
		{
			var parent = Helper.CreateGameObjectWithChildren(0);

			parent.transform.DestroyAllChildren();
			yield return null;

			Assert.AreEqual(0, parent.transform.childCount);
		}

		[UnityTest]
		public IEnumerator DestroyAllChildren_OneChildren_NoChildrenRemain()
		{
			var parent = Helper.CreateGameObjectWithChildren(1);

			parent.transform.DestroyAllChildren();
			yield return null;

			Assert.AreEqual(0, parent.transform.childCount);
		}

		[UnityTest]
		public IEnumerator DestroyAllChildren_ManyChildren_NoChildrenRemain()
		{
			var parent = Helper.CreateGameObjectWithChildren(11);

			parent.transform.DestroyAllChildren();
			yield return null;

			Assert.AreEqual(0, parent.transform.childCount);
		}

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
