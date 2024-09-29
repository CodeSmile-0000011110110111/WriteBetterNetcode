// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Extensions.UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Samples.Tests
{
	[DisallowMultipleComponent]
	public sealed class SkeletonVisualizer : MonoBehaviour
	{
		[SerializeField] private SkinnedMeshRenderer m_LinkedRenderer;
		[SerializeField] private Transform m_RootBone;
		[SerializeField] private PrimitiveType m_BonePrimitive = PrimitiveType.Cube;
		[SerializeField] private Single m_HipBonePrimitiveScale = 0.25f;
		[SerializeField] private Single m_BonePrimitiveScale = 0.5f;

		private void Start()
		{
			var shouldBonify = m_LinkedRenderer != null && m_LinkedRenderer.sharedMesh == null;
			if (shouldBonify)
				CreateBones(GetRootBone(), gameObject.layer);

#if !UNITY_EDITOR
			Destroy(this);
#endif
		}

		private Transform GetRootBone() => m_RootBone != null ? m_RootBone : transform;

		private void CreateBones(Transform t, Int32 layer)
		{
			// recurse first, since we modify the hierarchy
			foreach (Transform child in t)
				CreateBones(child, layer);

			var primitive = GameObject.CreatePrimitive(m_BonePrimitive);
			if (primitive.TryGetComponent<SphereCollider>(out var collider))
				collider.DestroyInAnyMode();

			var distanceToParent = m_HipBonePrimitiveScale;
			var direction = Vector3.forward;
			var halfPos = GetRootBone().position;
			if (t.parent != null && t != GetRootBone())
			{
				direction = t.parent.position - t.position;
				distanceToParent = direction.magnitude;
				halfPos = t.parent.position + direction * 0.5f;
			}

			primitive.layer = layer;

			var pt = primitive.transform;
			pt.parent = t;
			//pt.position = halfPos;
			pt.localPosition = Vector3.zero;
			pt.LookAt(t.parent, Vector3.up);
			//pt.rotation = Quaternion.LookRotation(direction, Vector3.forward);
			pt.localScale = Vector3.one * distanceToParent * m_BonePrimitiveScale;

#if UNITY_EDITOR
			m_BonePrimitives.Add(primitive.transform);
#endif
		}

#if UNITY_EDITOR
		[SerializeField] private Boolean m_AutoUpdateBones;
		[SerializeField] private Boolean m_ClickToCreateBones;
		[SerializeField] private Boolean m_ClickToDestroyBones;
		[SerializeField] private List<Transform> m_BonePrimitives = new();
#endif

#if UNITY_EDITOR
		private void OnValidate() => EditorCoroutineUtility.StartCoroutine(DelayedValidation(), this);

		private IEnumerator DelayedValidation()
		{
			yield return null;

			if (m_ClickToDestroyBones || m_ClickToCreateBones || m_AutoUpdateBones)
			{
				if (m_ClickToDestroyBones)
				{
					m_ClickToDestroyBones = false;
					m_AutoUpdateBones = false;
				}

				DestroyBonePrimitives();
			}

			if (m_ClickToCreateBones || m_AutoUpdateBones)
			{
				m_ClickToCreateBones = false;
				CreateBones(GetRootBone(), gameObject.layer);
			}
		}

		private void DestroyBonePrimitives()
		{
			foreach (var bonePrimitive in m_BonePrimitives)
				DestroyImmediate(bonePrimitive.gameObject);
			m_BonePrimitives.Clear();
		}
#endif
	}
}
