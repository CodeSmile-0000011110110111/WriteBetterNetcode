// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	[DisallowMultipleComponent]
	public abstract class ModularCharacterControllerBase : MonoBehaviour
	{
		protected CharacterController m_CharacterController;

		/// <summary>
		/// Must call base.Awake() when overridden!
		/// </summary>
		protected virtual void Awake() => m_CharacterController = TryMoveCharacterControllerToParent();

		/// <summary>
		/// Must call base.OnDestroy() when overridden!
		/// </summary>
		protected virtual void OnDestroy()
		{
			// remove char ctrl we added to parent
			Destroy(m_CharacterController);
			m_CharacterController = null;
		}

		/// <summary>
		/// Move the
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
			Destroy(sourceCtrl);

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
			dest.enabled = source.enabled;
		}
	}
}