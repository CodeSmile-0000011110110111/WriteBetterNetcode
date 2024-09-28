// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Weapons
{
	internal sealed class ActiveProjectiles : IEnumerable<ActiveProjectile>
	{
		private readonly List<ActiveProjectile> m_Projectiles = new();
		public Int32 Count => m_Projectiles.Count;
		public ActiveProjectile this[Int32 index] => m_Projectiles[index];
		public IEnumerator<ActiveProjectile> GetEnumerator() => m_Projectiles.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		internal void Add(ActiveProjectile projectile) => m_Projectiles.Add(projectile);
		internal void RemoveAt(Int32 i) => m_Projectiles.RemoveAt(i);
	}

	internal struct ActiveProjectile
	{
		public Transform Transform;
		public ProjectileData Data;
		public ProjectileRuntimeData RuntimeData;
	}
}
