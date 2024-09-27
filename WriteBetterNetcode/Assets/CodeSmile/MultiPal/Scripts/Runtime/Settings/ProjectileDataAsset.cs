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
		public LayerMask CollidesWithLayers = int.MaxValue;
		public QueryTriggerInteraction TriggerCollision = QueryTriggerInteraction.Ignore;

		[Header("Stats")]
		public float Damage;
		public float Speed;
		public float MaxLifetime;

		// FIXME: this does not belong inside
		public ProjectileRuntimeData RuntimeData;
	}

	public struct ProjectileRuntimeData
	{
		public float TimeToDie;
		public ulong OwnerClientId;
		public int PlayerIndex;
	}
}
