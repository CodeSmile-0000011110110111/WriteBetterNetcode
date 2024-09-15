// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Stats
{
	internal sealed class HitpointsStat : MonoBehaviour
	{
		[SerializeField] Hitpoints m_Hitpoints;

	}

	[Serializable]
	public sealed class Hitpoints
	{
		public float Current = 1f;
		public float Max = 1f;

		public Hitpoints(Single current, Single max)
		{
			Current = current;
			Max = max;
		}
	}
}
