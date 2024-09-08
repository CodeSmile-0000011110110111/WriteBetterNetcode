// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Animation;
using CodeSmile.MultiPal.PlayerController;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Samples.RoboKyle.Animator
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(UnityEngine.Animator))]
	public sealed class KyleAnimatorController : AnimatorControllerBase
	{
		private PlayerControllers m_PlayerControllers;
		private UnityEngine.Animator m_Animator;
		private AvatarAnimatorParameters m_KyleAnimParams;

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
			m_Animator = GetComponent<UnityEngine.Animator>();
			m_PlayerControllers = ComponentsRegistry.Get<PlayerControllers>();

			m_ParamInputMagnitude = UnityEngine.Animator.StringToHash("InputMagnitude");
			m_ParamMoveSpeed = UnityEngine.Animator.StringToHash("MoveSpeed");
			m_ParamIsGrounded = UnityEngine.Animator.StringToHash("IsGrounded");
			m_ParamIsFalling = UnityEngine.Animator.StringToHash("IsFalling");
			m_ParamTriggerJump = UnityEngine.Animator.StringToHash("TriggerJump");
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

		public override AvatarAnimatorParameters GetAnimatorParameters(Int32 playerIndex)
		{
			if (m_KyleAnimParams == null)
				m_KyleAnimParams = new KyleAvatarAnimatorParameters();

			var activeCtrl = m_PlayerControllers.GetActiveController(playerIndex);
			activeCtrl.AvatarAnimatorParameters = m_KyleAnimParams;

			return m_KyleAnimParams;
		}

		public override void SetAnimatorParameters(Int32 playerIndex, AvatarAnimatorParameters avatarAnimatorParameters)
		{
			Debug.Log($"set new animator params: {avatarAnimatorParameters}");
			m_KyleAnimParams = avatarAnimatorParameters;
		}
	}
}
