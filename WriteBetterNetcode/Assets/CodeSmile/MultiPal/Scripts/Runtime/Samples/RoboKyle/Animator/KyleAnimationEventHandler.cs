// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeSmile.MultiPal.Samples.RoboKyle.Animator
{
	[DisallowMultipleComponent]
	public sealed class KyleAnimationEventHandler : MonoBehaviour
	{
		[SerializeField] private AudioClip m_LandingAudioClip;
		[SerializeField] private AudioClip[] m_FootstepAudioClips;
		[SerializeField] [Range(0f, 1f)] private Single m_FootstepAudioVolume = 0.5f;
		[SerializeField] private Single m_ClipMinWeight = 0.5f;

		private void OnFootstep(AnimationEvent animationEvent)
		{
			if (animationEvent.animatorClipInfo.weight > m_ClipMinWeight)
			{
				if (m_FootstepAudioClips.Length > 0)
				{
					var index = Random.Range(0, m_FootstepAudioClips.Length);
					var clip = m_FootstepAudioClips[index];
					AudioSource.PlayClipAtPoint(clip, transform.position, m_FootstepAudioVolume);
				}
			}
		}

		private void OnLand(AnimationEvent animationEvent)
		{
			if (animationEvent.animatorClipInfo.weight > m_ClipMinWeight)
				AudioSource.PlayClipAtPoint(m_LandingAudioClip, transform.position, m_FootstepAudioVolume);
		}
	}
}
