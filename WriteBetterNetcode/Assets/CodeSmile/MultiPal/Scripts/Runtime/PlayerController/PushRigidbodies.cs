// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.PlayerController
{
	public class PushRigidbodies : MonoBehaviour
	{
		[Range(0.1f, 10f)] public Single m_PushForce = 3f;

		private void OnControllerColliderHit(ControllerColliderHit hit) => PushRigidBodies(hit);

		private void PushRigidBodies(ControllerColliderHit hit)
		{
			var hitBody = hit.collider.attachedRigidbody;
			if (hitBody == null || hitBody.isKinematic)
				return;

			var pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
			hitBody.AddForce(pushDir * m_PushForce, ForceMode.Impulse);
		}
	}
}
