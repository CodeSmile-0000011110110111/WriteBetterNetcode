// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Animation;
using CodeSmile.MultiPal.PlayerController;
using CodeSmile.MultiPal.Players;
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
		private AvatarAnimatorParameters m_AnimParams;

		private PlayerClient m_ClientSide;

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

		public override void Init(Int32 playerIndex, Boolean isOwner)
		{
			PlayerIndex = playerIndex;
			IsOwner = isOwner;

			if (m_AnimParams == null)
				m_AnimParams = new AvatarAnimatorParameters();

			if (isOwner)
			{
				// hook up with character controller
				var activeCtrl = m_PlayerControllers.GetActiveController(playerIndex);
				activeCtrl.AvatarAnimatorParameters = m_AnimParams;
			}
		}

		private void Awake()
		{
			m_PlayerControllers = ComponentsRegistry.Get<PlayerControllers>();
			m_ClientSide = GetComponentInParent<PlayerClient>();
			m_Animator = GetComponent<UnityEngine.Animator>();

			m_ParamInputMagnitude = UnityEngine.Animator.StringToHash("InputMagnitude");
			m_ParamMoveSpeed = UnityEngine.Animator.StringToHash("MoveSpeed");
			m_ParamIsGrounded = UnityEngine.Animator.StringToHash("IsGrounded");
			m_ParamIsFalling = UnityEngine.Animator.StringToHash("IsFalling");
			m_ParamTriggerJump = UnityEngine.Animator.StringToHash("TriggerJump");
		}

		private void OnEnable() => m_ClientSide.AnimatorController = this;

		private void OnDisable()
		{
			if (m_ClientSide.AnimatorController == this)
				m_ClientSide.AnimatorController = null;
		}

		private void LateUpdate()
		{
			if (m_AnimParams != null)
			{
				MoveSpeed = m_AnimParams.MoveSpeed;
				InputMagnitude = m_AnimParams.InputMagnitude;
				IsGrounded = m_AnimParams.IsGrounded;
				IsFalling = m_AnimParams.IsFalling;
				TriggerJump = m_AnimParams.TriggerJump;

				if (IsOwner)
					m_ClientSide.SendAnimatorParametersToNonOwners(m_AnimParams.Parameters);
			}
		}

		public override void RemoteAnimatorParametersReceived(Byte[] animatorParameters) =>
			m_AnimParams.Parameters = animatorParameters;
	}
}
