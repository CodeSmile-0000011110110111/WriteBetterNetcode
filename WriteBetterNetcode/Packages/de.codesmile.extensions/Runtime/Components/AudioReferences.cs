// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.Components
{
	public class AudioReferences : MonoSingleton<AudioReferences>
	{
		[SerializeField] private AudioListener m_AudioListener;

		public AudioListener Listener => m_AudioListener;

		protected override void Awake()
		{
			base.Awake();

			if (m_AudioListener == null)
				m_AudioListener = GetComponentInChildren<AudioListener>();

#if !UNITY_SERVER
			if (m_AudioListener == null)
				throw new MissingComponentException(nameof(AudioListener));
#endif
		}
	}
}
