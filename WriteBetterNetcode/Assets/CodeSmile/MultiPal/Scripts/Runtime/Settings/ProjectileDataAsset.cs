// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Settings
{
	[CreateAssetMenu(fileName = nameof(ProjectileDataAsset), menuName = "CodeSmile/" + nameof(ProjectileDataAsset), order = 0)]
	public sealed class ProjectileDataAsset : ScriptableObject
	{
		public ProjectileData Data;
	}

	[Serializable]
	public sealed class ProjectileData
	{
		public GameObject ProjectilePrefab;
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
