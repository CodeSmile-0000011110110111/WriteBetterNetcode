// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using UnityEngine;

namespace CodeSmile.Components
{
	public class WhenTimeElapsed : MonoBehaviour
	{
		public enum Action
		{
			DestroyGameObject,
		}

		[SerializeField] private Single m_SecondsToAction = 3f;
		[SerializeField] private Action m_TimeOutAction;

		private Single m_TargetTime;

		private void OnEnable()
		{
			StopAllCoroutines();
			StartCoroutine(WhenYourTimeComesYouDie());
		}

		private IEnumerator WhenYourTimeComesYouDie()
		{
			m_TargetTime = Time.time + m_SecondsToAction;

			yield return new WaitUntil(() => Time.time > m_TargetTime);

			PerformAction();
		}

		private void PerformAction()
		{
			switch (m_TimeOutAction)
			{
				case Action.DestroyGameObject:
					Destroy(gameObject);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
