// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Animator))]
	public sealed class KyleAnimatorController : MonoBehaviour
	{
		private Animator m_Animator;
		private IAnimatorParametersProvider m_ParamsProvider;
		private KyleAnimatorParameters m_KyleAnimParams;

		private Int32 m_ParamSpeed;
		private Int32 m_ParamMotionSpeed;
		private Int32 m_ParamGrounded;
		private Int32 m_ParamFreeFall;
		private Int32 m_ParamJump;

		public Single Speed { set => m_Animator.SetFloat(m_ParamSpeed, value); }

		public Single MotionSpeed { set => m_Animator.SetFloat(m_ParamMotionSpeed, value); }

		public Boolean Grounded
		{
			set => m_Animator.SetBool(m_ParamGrounded, value);
			// m_Animator.SetBool(m_ParamJump, false);
			// m_Animator.SetBool(m_ParamFreeFall, !value);
		}
		public Boolean FreeFall
		{
			set =>
				// m_Animator.SetBool(m_ParamGrounded, false);
				// m_Animator.SetBool(m_ParamJump, false);
				m_Animator.SetBool(m_ParamFreeFall, value);
		}
		public Boolean Jump
		{
			set =>
				// m_Animator.SetBool(m_ParamGrounded, false);
				m_Animator.SetBool(m_ParamJump, value);
			// m_Animator.SetBool(m_ParamFreeFall, false);
		}

		private void OnEnable()
		{
			// reset anim state every time we get enabled
			m_KyleAnimParams = new KyleAnimatorParameters();
		}

		private void OnDisable() => UnassignAnimatorParameters();

		private void Awake()
		{
			m_Animator = GetComponent<Animator>();
			m_ParamsProvider = GetComponentInParent<IAnimatorParametersProvider>();
			if (m_ParamsProvider == null)
				throw new MissingComponentException($"parent expected to have {nameof(IAnimatorParametersProvider)}");

			m_ParamSpeed = Animator.StringToHash("Speed");
			m_ParamMotionSpeed = Animator.StringToHash("MotionSpeed");
			m_ParamGrounded = Animator.StringToHash("Grounded");
			m_ParamFreeFall = Animator.StringToHash("FreeFall");
			m_ParamJump = Animator.StringToHash("Jump");
		}

		private void LateUpdate()
		{
			if (m_ParamsProvider.AnimatorParameters == null)
			{
				m_ParamsProvider.AnimatorParameters = m_KyleAnimParams;
			}

			Apply(m_KyleAnimParams);
		}

		private void UnassignAnimatorParameters()
		{
			if (m_ParamsProvider.AnimatorParameters == m_KyleAnimParams)
				m_ParamsProvider.AnimatorParameters = null;
		}

		public void Apply(KyleAnimatorParameters kyleAnimParams)
		{
			Speed = kyleAnimParams.CurrentSpeed;
			MotionSpeed = kyleAnimParams.InputMove.magnitude;
			Grounded = kyleAnimParams.IsGrounded;
			FreeFall = kyleAnimParams.IsFalling;
			Jump = kyleAnimParams.InputJump;
		}
	}
}
