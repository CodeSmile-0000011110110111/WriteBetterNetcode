// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components.Utility
{
	public class OneTimeTaskBehaviour : MonoBehaviour
	{
		[SerializeField]
		private OnTaskPerformed m_OnTaskPerformed = OnTaskPerformed.DestroyComponent;

		protected void TaskPerformed()
		{
			switch (m_OnTaskPerformed)
			{
				case OnTaskPerformed.DoNothing:
					break;
				case OnTaskPerformed.DestroyComponent:
					Destroy(this);
					break;
				case OnTaskPerformed.DestroyGameObject:
					Destroy(gameObject);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(m_OnTaskPerformed));
			}
		}

		private enum OnTaskPerformed
		{
			DoNothing,
			DestroyComponent,
			DestroyGameObject,
		}
	}
}
