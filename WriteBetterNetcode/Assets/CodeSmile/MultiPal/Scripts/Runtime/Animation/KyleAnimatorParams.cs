// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.CodeSmile.MultiPal.Animation
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Animator))]
	public sealed class KyleAnimatorParams : MonoBehaviour
	{
		private Animator m_Animator;

		private Int32 m_ParamSpeed;
		private Int32 m_ParamMotionSpeed;
		private Int32 m_ParamGrounded;
		private Int32 m_ParamJump;
		private Int32 m_ParamFreeFall;

		public Single Speed { set => m_Animator.SetFloat(m_ParamSpeed, value); }

		public Single MotionSpeed { set => m_Animator.SetFloat(m_ParamMotionSpeed, value); }

		// highest priority
		public Boolean Grounded
		{
			set
			{
				m_Animator.SetBool(m_ParamGrounded, value);
				// m_Animator.SetBool(m_ParamJump, false);
				// m_Animator.SetBool(m_ParamFreeFall, !value);
			}
		}
		// medium priority
		public Boolean Jump
		{
			set
			{
				// m_Animator.SetBool(m_ParamGrounded, false);
				m_Animator.SetBool(m_ParamJump, value);
				// m_Animator.SetBool(m_ParamFreeFall, false);
			}
		}
		// lowest priority
		public Boolean FreeFall
		{
			set
			{
				// m_Animator.SetBool(m_ParamGrounded, false);
				// m_Animator.SetBool(m_ParamJump, false);
				m_Animator.SetBool(m_ParamFreeFall, value);
			}
		}

		private void Awake()
		{
			m_Animator = GetComponent<Animator>();
			m_ParamSpeed = Animator.StringToHash("Speed");
			m_ParamMotionSpeed = Animator.StringToHash("MotionSpeed");
			m_ParamGrounded = Animator.StringToHash("Grounded");
			m_ParamJump = Animator.StringToHash("Jump");
			m_ParamFreeFall = Animator.StringToHash("FreeFall");
		}
	}
}
