// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Players.Controllers
{
	// TODO: consider if KinematicControllers should be placed at the scene root, all together (like Cameras)
	// applying the CharacterController and copying properties would be done by PlayerKinematics
	// benefits? all controllers bundled together, easily accessible; code is simpler; better encapsulation of concerns++
	// drawbacks? .. not sure

	[DisallowMultipleComponent]
	public abstract class PlayerControllerBase : MonoBehaviour, GeneratedInput.IPlayerKinematicsActions
	{
		[SerializeField] private Vector3 m_MotionSensitivity = Vector3.one;

		public Transform Target { get; set; }

		/// <summary>
		///     The target's character controller - if one is being used.
		/// </summary>
		public CharacterController CharacterController { get; private set; }

		public Vector3 Velocity { get; protected set; }
		public Vector3 MotionSensitivity
		{
			get => m_MotionSensitivity;
			set => m_MotionSensitivity = value;
		}

		/// <summary>
		///     Gets Vector2 from an appropriate InputAction if the action is performed. Otherwise Vector2.zero.
		/// </summary>
		/// <param name="context"></param>
		/// <returns>InputAction vector or Vector2.zero if action not performed</returns>
		protected static Vector2 GetHorizontalVelocity(InputAction.CallbackContext context) =>
			context.performed ? context.ReadValue<Vector2>() : Vector2.zero;

		public virtual void OnMove(InputAction.CallbackContext context) {}
		public virtual void OnLook(InputAction.CallbackContext context) {}
		public virtual void OnCrouch(InputAction.CallbackContext context) {}
		public virtual void OnJump(InputAction.CallbackContext context) {}
		public virtual void OnSprint(InputAction.CallbackContext context) {}

		/// <summary>
		///     Must call base.Awake() when overridden!
		/// </summary>
		protected virtual void OnEnable()
		{
			// if placed at root, assume "standalone" mode
			if (transform.parent == null || Target == null)
				Target = transform;

			TryMoveCharacterControllerToTarget(Target);

			// character controller may be on a different object
			if (CharacterController != null)
				CharacterController.enabled = true;
		}

		/// <summary>
		///     Must call base.Awake() when overridden!
		/// </summary>
		protected virtual void OnDisable()
		{
			// character controller may be on a different object
			if (CharacterController != null)
			{
				CharacterController.enabled = false;
				CharacterController = null;
			}
		}

		/// <summary>
		///     Move the
		/// </summary>
		/// <returns></returns>
		private void TryMoveCharacterControllerToTarget(Transform target)
		{
			CharacterController = null;

			// check if we are using the CharacterController component
			var ourCharCtrl = GetComponent<CharacterController>();
			if (ourCharCtrl == null)
				return;

			// if CharCtrl present on target, use that, otherwise add one
			if (!target.TryGetComponent<CharacterController>(out var targetCharCtrl))
				targetCharCtrl = target.gameObject.AddComponent<CharacterController>();

			CopyInspectorProperties(ourCharCtrl, targetCharCtrl);
			ourCharCtrl.enabled = false; // make sure the source ctrl doesn't get in the way

			CharacterController = targetCharCtrl;
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
		protected void Move() => CharacterController.Move(Velocity);

		/// <summary>
		///     Apply 2D vector's X/Y components to X/Z components of Velocity. The Y component remains unchanged.
		/// </summary>
		/// <param name="horizontalVelocity"></param>
		protected void SetHorizontalVelocity(Vector2 horizontalVelocity) =>
			Velocity = new Vector3(horizontalVelocity.x, Velocity.y, horizontalVelocity.y);

		/// <summary>
		///     Assigns value to Velocity's Y component and leaves Velocity X/Z components unchanged.
		/// </summary>
		/// <param name="verticalVelocity"></param>
		protected void SetVerticalVelocity(Single verticalVelocity) =>
			Velocity = new Vector3(Velocity.x, verticalVelocity, Velocity.z);

		/// <summary>
		///     Adds to the Velocity's Y component. Leaves X/Z unchanged.
		/// </summary>
		/// <param name="verticalVelocity"></param>
		protected void AddVerticalVelocity(Single verticalVelocity) =>
			Velocity = new Vector3(Velocity.x, Velocity.y + verticalVelocity, Velocity.z);
	}
}
