// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.BetterNetcode.Input;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeSmile.Players.Controllers
{
	[DisallowMultipleComponent]
	public abstract class KinematicControllerBase : MonoBehaviour, GeneratedInput.IPlayerKinematicsActions
	{
		[SerializeField] private Vector3 m_MotionSensitivity = Vector3.one;

		[SerializeField] private Int32 m_PlayerIndex = -1;
		protected CharacterController m_CharacterController;
		public Int32 PlayerIndex => m_PlayerIndex;

		protected Vector3 Velocity { get; set; }
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

		public virtual void OnActivate(Int32 playerIndex) => m_PlayerIndex = playerIndex;
		public virtual void OnDeactivate(Int32 playerIndex) {}
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
			m_CharacterController = TryMoveCharacterControllerToParent();
			m_CharacterController.enabled = true;
		}

		/// <summary>
		///     Must call base.Awake() when overridden!
		/// </summary>
		protected virtual void OnDisable()
		{
			if (m_CharacterController != null)
			{
				m_CharacterController.enabled = false;
				m_CharacterController = null;
			}
		}

		/// <summary>
		///     Move the
		/// </summary>
		/// <returns></returns>
		private CharacterController TryMoveCharacterControllerToParent()
		{
			var sourceCtrl = GetComponent<CharacterController>();
			if (sourceCtrl == null)
				throw new MissingComponentException("CharacterController component required");

			// if not parented we use our CharCtrl
			var parent = transform.parent?.gameObject;
			if (parent == null)
				return sourceCtrl;

			// if CharCtrl present on parent, use that otherwise add one
			if (!parent.TryGetComponent<CharacterController>(out var parentCtrl))
				parentCtrl = parent.AddComponent<CharacterController>();

			CopyInspectorProperties(sourceCtrl, parentCtrl);
			sourceCtrl.enabled = false; // make sure the source ctrl doesn't get in the way

			return parentCtrl;
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
		protected void Move() => m_CharacterController.Move(Velocity);

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
