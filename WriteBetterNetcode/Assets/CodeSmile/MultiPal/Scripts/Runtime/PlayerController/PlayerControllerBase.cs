// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using CodeSmile.MultiPal.Animation;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

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
		[SerializeField] private Vector3 m_TranslationSensitivity = Vector3.one;
		[SerializeField] private Vector3 m_RotationSensitivity = Vector3.one;

		public AnimatorParametersBase AnimatorParameters { get; set; }

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

		public Vector3 TranslationSensitivity
		{
			get => m_TranslationSensitivity;
			set => m_TranslationSensitivity = value;
		}
		public Vector3 RotationSensitivity
		{
			get => m_RotationSensitivity;
			set => m_RotationSensitivity = value;
		}

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
		}

		/// <summary>
		///     Must call base.Awake() when overridden!
		/// </summary>
		protected virtual void OnDisable()
		{
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

		/// <summary>
		///     Moves character with velocity.
		/// </summary>
		protected void Move()
		{
			if (Velocity != Vector3.zero)
			{
				CharController.Move(Velocity);
				MotionTarget.forward = Velocity.normalized;
			}
		}

		protected void Rotate()
		{
			//Target.localRotation = Rotation;
			//Target.Rotate(0f, m_Pan.Value, 0f);
		}
	}
}
