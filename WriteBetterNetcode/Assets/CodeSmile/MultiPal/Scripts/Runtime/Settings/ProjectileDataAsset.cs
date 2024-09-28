// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	[CreateAssetMenu(fileName = nameof(ProjectileDataAsset), menuName = "CodeSmile/Weapons/" + nameof(ProjectileDataAsset))]
	public sealed class ProjectileDataAsset : ScriptableObject
	{
		public ProjectileData Data;
	}

	[Serializable]
	public sealed class ProjectileData
	{
		[Header("Visualization")]
		public GameObject ProjectilePrefab;

		[Header("Collision")]
		public GameObject ImpactPrefab;
		public LayerMask CollidesWithLayers = Int32.MaxValue;
		public QueryTriggerInteraction TriggerInteraction = QueryTriggerInteraction.Ignore;

		[Header("Stats")]
		public Single Damage;
		public Single Speed;
		public Single Lifetime;

		public ProjectileRuntimeData RuntimeData;
	}

	public struct ProjectileRuntimeData
	{
		public Single TimeToDie;
		public UInt64 OwnerClientId;
		public Int32 PlayerIndex;
	}
}
