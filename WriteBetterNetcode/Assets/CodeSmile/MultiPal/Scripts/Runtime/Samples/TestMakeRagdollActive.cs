using System;
using UnityEngine;

public class TestMakeRagdollActive : MonoBehaviour
{
	[SerializeField] private Animator m_Animator;
	[SerializeField] private Transform m_SkeletonRoot;
	[SerializeField] private Transform m_RagdollSkeletonRoot;
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.B))
		{
			ActivateRagdoll();
		}
	}

	private void ActivateRagdoll()
	{
		Debug.Log("MAKE DOLL");

		m_Animator.StopPlayback();
		m_Animator.enabled = false;

		if (m_SkeletonRoot != null)
			m_SkeletonRoot.gameObject.SetActive(false);

		m_RagdollSkeletonRoot.gameObject.SetActive(true);
		var bodies = m_RagdollSkeletonRoot.GetComponentsInChildren<Rigidbody>();
		foreach (var body in bodies)
		{
			body.isKinematic = false;
		}
	}

	private void Awake()
	{
		var bodies = m_RagdollSkeletonRoot.GetComponentsInChildren<Rigidbody>();
		foreach (var body in bodies)
		{
			body.isKinematic = true;
		}
	}
}
