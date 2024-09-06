// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Utility;
using CodeSmile.MultiPal.Animation;
using CodeSmile.MultiPal.Players.Controllers;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Samples.Kyle
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Animator))]
	public sealed class KyleAnimatorController : AnimatorControllerBase
	{
		private PlayerControllers m_PlayerControllers;
		private Animator m_Animator;
		private KyleAnimatorParameters m_KyleAnimParams;

		private Int32 m_ParamSpeed;
		private Int32 m_ParamMotionSpeed;
		private Int32 m_ParamGrounded;
		private Int32 m_ParamFreeFall;
		private Int32 m_ParamJump;

		public Single Speed { set => m_Animator.SetFloat(m_ParamSpeed, value); }
		public Single MotionSpeed { set => m_Animator.SetFloat(m_ParamMotionSpeed, value); }
		public Boolean Grounded { set => m_Animator.SetBool(m_ParamGrounded, value); }
		public Boolean FreeFall { set => m_Animator.SetBool(m_ParamFreeFall, value); }
		public Boolean Jump { set => m_Animator.SetBool(m_ParamJump, value); }

		private void Awake()
		{
			m_Animator = GetComponent<Animator>();
			m_PlayerControllers = ComponentsRegistry.Get<PlayerControllers>();

			m_ParamSpeed = Animator.StringToHash("Speed");
			m_ParamMotionSpeed = Animator.StringToHash("MotionSpeed");
			m_ParamGrounded = Animator.StringToHash("Grounded");
			m_ParamFreeFall = Animator.StringToHash("FreeFall");
			m_ParamJump = Animator.StringToHash("Jump");
		}

		private void LateUpdate()
		{
			if (m_KyleAnimParams != null)
			{
				Speed = m_KyleAnimParams.CurrentSpeed;
				MotionSpeed = m_KyleAnimParams.InputMove.magnitude;
				Grounded = m_KyleAnimParams.IsGrounded;
				FreeFall = m_KyleAnimParams.IsFalling;
				Jump = m_KyleAnimParams.InputJump;
			}
		}

		public override void OnAssignAnimationData(Int32 playerIndex)
		{
			// reset anim state every time we get enabled
			var activeCtrl = m_PlayerControllers.GetActiveController(playerIndex);
			activeCtrl.AnimatorParameters = m_KyleAnimParams = new KyleAnimatorParameters();
		}
	}
}
