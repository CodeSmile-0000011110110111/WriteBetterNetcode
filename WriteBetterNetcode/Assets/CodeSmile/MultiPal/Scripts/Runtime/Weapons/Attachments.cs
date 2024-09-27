// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Weapons
{
	/// <summary>
	///     Allows to define attachments and then spawn objects into a given attachment slot index. One object per slot.
	/// </summary>
	[DisallowMultipleComponent]
	public class Attachments : MonoBehaviour
	{
		[SerializeField] private Transform[] m_AttachSlots;

		private GameObject[] m_AttachedObjects;

		private void Awake() => m_AttachedObjects = new GameObject[m_AttachSlots.Length];

		/// <summary>
		///     Instantiates the prefab object into the given slot. Destroys the already-existing object. If prefab is null,
		///     will destroy (clear) the attached object.
		/// </summary>
		/// <param name="attachmentIndex">The slot to instantiate the object into.</param>
		/// <param name="prefab">The object to instantiate in the slot. If null the existing object in this slot is destroyed.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">Attachment index is not in range of attach slots.</exception>
		public GameObject SetAttachment(Int32 attachmentIndex, GameObject prefab)
		{
			if (attachmentIndex < 0 || attachmentIndex >= m_AttachSlots.Length)
			{
				Debug.LogWarning($"Attachment index {attachmentIndex} is out of range [{m_AttachSlots.Length}].");
				return null;
			}

			var attachment = m_AttachSlots[attachmentIndex];
			if (attachment == null)
			{
				Debug.LogWarning($"Attachment at index {attachmentIndex} is null.");
				return null;
			}

			var attachedObject = prefab != null ? Instantiate(prefab, attachment) : null;

			Destroy(m_AttachedObjects[attachmentIndex]);
			m_AttachedObjects[attachmentIndex] = attachedObject;

			return attachedObject;
		}
	}
}
