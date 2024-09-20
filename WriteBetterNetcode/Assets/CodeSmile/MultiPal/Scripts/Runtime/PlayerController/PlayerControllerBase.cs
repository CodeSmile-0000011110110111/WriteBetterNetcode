// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Animation;
using CodeSmile.MultiPal.Input;
using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace CodeSmile.MultiPal.PlayerController
{
	[DisallowMultipleComponent]
	public abstract class PlayerControllerBase : MonoBehaviour, GeneratedInput.IPlayerKinematicsActions,
		IAnimatorParametersProvider
	{
		[Header("Translation Axis")]
		[SerializeField] protected InputAxis m_Sideways = InputAxis.DefaultMomentary;
		[SerializeField] protected InputAxis m_Vertical = InputAxis.DefaultMomentary;
		[SerializeField] protected InputAxis m_Forward = InputAxis.DefaultMomentary;

		[Header("Rotation Axis")]
		[Tooltip("Up/Down rotation. Equates to X axis rotation.")]
		[SerializeField] protected InputAxis m_Tilt = DefaultTilt;
		[Tooltip("Sideways rotation. Equates to Y axis rotation.")]
		[SerializeField] protected InputAxis m_Pan = DefaultPan;
		[Tooltip("Sideways rotation. Equates to Y axis rotation.")]
		[SerializeField] protected InputAxis m_Roll = DefaultRoll;

		[Header("Sensitivity Scaling")]
		[FormerlySerializedAs("m_TranslationSensitivity")]
		[SerializeField] private Vector3 m_MoveSensitivity = Vector3.one * 0.1f;
		[FormerlySerializedAs("m_RotationSensitivity")]
		[SerializeField] private Vector3 m_LookSensitivity = Vector3.one * 0.1f;

		public AvatarAnimatorParameters AnimatorParameters { get; set; }

		private static InputAxis DefaultTilt => new()
		{
			Value = 0f, Range = new Vector2(-85f, 85f), Wrap = false, Center = 0f,
			Restrictions = InputAxis.RestrictionFlags.NoRecentering,
		};
		private static InputAxis DefaultPan => new()
		{
			Value = 0f, Range = new Vector2(-180f, 180f), Wrap = true, Center = 0f,
			Restrictions = InputAxis.RestrictionFlags.NoRecentering,
		};
		private static InputAxis DefaultRoll => new()
		{
			Value = 0f, Range = new Vector2(-60f, 60f), Wrap = false, Center = 0f,
			Restrictions = InputAxis.RestrictionFlags.NoRecentering,
		};

		public Transform MotionTarget { get; set; }
		public Transform CameraTarget { get; set; }

		/// <summary>
		///     The target's character controller - if one is being used.
		/// </summary>
		public CharacterController CharController { get; private set; }

		public Vector3 Velocity => CharController != null
			? CharController.velocity
			: new Vector3(m_Sideways.Value, m_Vertical.Value, m_Forward.Value);

		public Vector3 MoveSensitivity
		{
			get => m_MoveSensitivity;
			set => m_MoveSensitivity = value;
		}
		public Vector3 LookSensitivity
		{
			get => m_LookSensitivity;
			set => m_LookSensitivity = value;
		}
		public Int32 PlayerIndex { get; set; }

		public virtual void OnMove(InputAction.CallbackContext context) {}
		public virtual void OnLook(InputAction.CallbackContext context) {}

		public virtual void OnCrouch(InputAction.CallbackContext context) {}
		public virtual void OnJump(InputAction.CallbackContext context) {}
		public virtual void OnSprint(InputAction.CallbackContext context) {}

		private void OnValidate()
		{
			m_Tilt.Validate();
			m_Pan.Validate();
		}

		/// <summary>
		///     Must call base.Awake() when overridden!
		/// </summary>
		protected virtual void OnEnable()
		{
			// if placed at root, assume "standalone" mode
			if (transform.parent == null || MotionTarget == null)
				MotionTarget = transform;

			TryMoveCharacterControllerToTarget(MotionTarget);

			// character controller may be on a different object
			if (CharController != null)
				CharController.enabled = true;

			// apply initial values
			if (CameraTarget != null)
			{
				m_Tilt.Value = CameraTarget.rotation.eulerAngles.x;
				m_Tilt.Validate();
			}

			if (MotionTarget != null)
			{
				m_Pan.Value = MotionTarget.rotation.eulerAngles.y;
				m_Pan.Validate();
			}

			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.SetPlayerKinematicsCallback(PlayerIndex, this);
		}

		/// <summary>
		///     Must call base.Awake() when overridden!
		/// </summary>
		protected virtual void OnDisable()
		{
			var inputUsers = ComponentsRegistry.Get<InputUsers>();
			inputUsers.SetPlayerKinematicsCallback(PlayerIndex, null);

			// character controller may be on a different object
			if (CharController != null)
			{
				CharController.enabled = false;
				CharController = null;
			}
		}

		/// <summary>
		///     Move the
		/// </summary>
		/// <returns></returns>
		private void TryMoveCharacterControllerToTarget(Transform target)
		{
			CharController = null;

			// check if we are using the CharacterController component
			var ourCharCtrl = GetComponent<CharacterController>();
			if (ourCharCtrl == null)
				return;

			// if CharCtrl present on target, use that, otherwise add one
			if (!target.TryGetComponent<CharacterController>(out var targetCharCtrl))
				targetCharCtrl = target.gameObject.AddComponent<CharacterController>();

			CopyInspectorProperties(ourCharCtrl, targetCharCtrl);
			ourCharCtrl.enabled = false; // make sure the source ctrl doesn't get in the way

			CharController = targetCharCtrl;
		}

		/// <summary>
		///     Copies all Inspector-editable CharacterController properties to another CharacterController.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="dest"></param>
		private void CopyInspectorProperties(CharacterController source, CharacterController dest)
		{
			dest.slopeLimit = source.slopeLimit;
			dest.stepOffset = source.stepOffset;
			dest.skinWidth = source.skinWidth;
			dest.minMoveDistance = source.minMoveDistance;
			dest.center = source.center;
			dest.radius = source.radius;
			dest.height = source.height;
			dest.layerOverridePriority = source.layerOverridePriority;
			dest.includeLayers = source.includeLayers;
			dest.excludeLayers = source.excludeLayers;
		}
	}
}
