// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Animation;
using CodeSmile.MultiPal.PlayerController;
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
		private AnimatorParametersBase m_KyleAnimParams;

		private Int32 m_ParamMoveSpeed;
		private Int32 m_ParamInputMagnitude;
		private Int32 m_ParamIsGrounded;
		private Int32 m_ParamIsFalling;
		private Int32 m_ParamTriggerJump;

		private Single InputMagnitude { set => m_Animator.SetFloat(m_ParamInputMagnitude, value); }
		private Single MoveSpeed { set => m_Animator.SetFloat(m_ParamMoveSpeed, value); }
		private Boolean IsGrounded { set => m_Animator.SetBool(m_ParamIsGrounded, value); }
		private Boolean IsFalling { set => m_Animator.SetBool(m_ParamIsFalling, value); }
		private Boolean TriggerJump { set => m_Animator.SetBool(m_ParamTriggerJump, value); }

		private void Awake()
		{
			m_Animator = GetComponent<Animator>();
			m_PlayerControllers = ComponentsRegistry.Get<PlayerControllers>();

			m_ParamInputMagnitude = Animator.StringToHash("InputMagnitude");
			m_ParamMoveSpeed = Animator.StringToHash("MoveSpeed");
			m_ParamIsGrounded = Animator.StringToHash("IsGrounded");
			m_ParamIsFalling = Animator.StringToHash("IsFalling");
			m_ParamTriggerJump = Animator.StringToHash("TriggerJump");
		}

		private void LateUpdate()
		{
			if (m_KyleAnimParams != null)
			{
				MoveSpeed = m_KyleAnimParams.MoveSpeed;
				InputMagnitude = m_KyleAnimParams.InputMagnitude;
				IsGrounded = m_KyleAnimParams.IsGrounded;
				IsFalling = m_KyleAnimParams.IsFalling;
				TriggerJump = m_KyleAnimParams.TriggerJump;
			}
		}

		public override AnimatorParametersBase GetAnimatorParameters(Int32 playerIndex)
		{
			if (m_KyleAnimParams == null)
				m_KyleAnimParams = new KyleAnimatorParameters();

			var activeCtrl = m_PlayerControllers.GetActiveController(playerIndex);
			activeCtrl.AnimatorParameters = m_KyleAnimParams;

			return m_KyleAnimParams;
		}

		public override void SetAnimatorParameters(Int32 playerIndex, AnimatorParametersBase animatorParameters)
		{
			Debug.Log($"set new animator params: {animatorParameters}");
			m_KyleAnimParams = animatorParameters;
		}
	}
}
