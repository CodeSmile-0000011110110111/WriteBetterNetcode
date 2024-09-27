// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.CodeSmile.Extensions.UnityEngine;
using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Settings;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Weapons
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ProjectilesClient), typeof(ProjectilesServer))]
	public sealed class Projectiles : MonoBehaviour
	{
		private static readonly RaycastHit[] s_RaycastHits = new RaycastHit[16];

		private readonly List<ActiveProjectile> m_Projectiles = new();

		private void Awake() => ComponentsRegistry.Set(this);

		private void Update()
		{
			/* Networking changes:
			 - projectiles are net objects or not?
				yes: server authority, higher traffic
				no: less traffic
					client can't destroy bullet that missed locally but hit on server
					client can't un-destroy bullet that hit locally but missed on server
				support both?
					server tells clients about impact either way

				mode determined by ProjectileDataAsset if prefab has NetworkObject component


				server auth:
				FireWeapon
					start/stop firing RPC to server
						toggles firing state (automatic), non-automatic same but may ignore stop fire event
						get position, rotation, projectile from player's weapon
						net spawn projectile

					server ticks and destroys projectiles (net tick)
					server sends impacts to clients (not EOL, synched implicitly through Destroy)

				client auth (COOP only!):
				FireWeapon
					start/stop firing RPC to everyone
						get position, rotation, projectile from player's weapon
							(will cause latency deviation for every client, for coop it's okay
							(could also send: position, rotation, projectile data)
						local spawn projectile

					every client ticks and destroys own projectiles (Update)
					every client sends its projectile impacts to clients
						EOL: projectiles need to destroy themselves after a set time
			*/


			var currentTime = Time.time;
			var deltaTime = Time.deltaTime;
			RaycastHit hit = default;

			var projectileCount = m_Projectiles.Count;
			for (var i = projectileCount - 1; i >= 0; i--)
			{
				var projectile = m_Projectiles[i];
				var projectileTransform = projectile.Transform;
				var runtimeData = projectile.RuntimeData;
				var endOfLife = currentTime >= runtimeData.TimeToDie;

				if (!endOfLife)
				{
					var forward = projectileTransform.forward;
					var previousPosition = projectileTransform.position;
					var distanceTravelled = projectile.Data.Speed * deltaTime;
					var nextPosition = previousPosition + forward * distanceTravelled;

					var travelRay = new Ray(previousPosition, forward);
					if (CheckCollision(projectile, travelRay, distanceTravelled, out hit))
					{
						endOfLife = true;

						// use less than full distance to keep impact fx a little off-surface (prevent z-fighting)
						nextPosition = previousPosition + forward * (hit.distance * 0.99f);

						// assumption: impact fx objects destroy themselves
						Instantiate(projectile.Data.ImpactPrefab, nextPosition, Quaternion.LookRotation(hit.normal));
					}
					else
						projectileTransform.position = nextPosition;
				}

				if (endOfLife)
				{
					Destroy(projectile.Transform.gameObject);
					m_Projectiles.RemoveAt(i);
				}
			}
		}

		public void FireWeapon(WeaponData weaponData, Transform[] spawnPoints)
		{
			if (spawnPoints == null || spawnPoints.Length != 1)
				throw new ArgumentException("Spawn points must be exactly 1 for now!");

			var projectileData = weaponData.Projectile.Data;
			var projectileRuntimeData = projectileData.RuntimeData;
			projectileRuntimeData.TimeToDie = Time.time + projectileData.MaxLifetime;

			var parent = spawnPoints[0];
			var projectileObj = Instantiate(projectileData.ProjectilePrefab, parent.position, parent.rotation);
			projectileObj.transform.parent = transform;

			m_Projectiles.Add(new()
			{
				Transform = projectileObj.transform,
				Data = projectileData,
				RuntimeData = projectileRuntimeData,
			});
		}

		private Boolean CheckCollision(ActiveProjectile projectile, Ray travelRay, Single distance, out RaycastHit hit)
		{
			var data = projectile.Data;
			var numHits = PhysicsExt.RaycastNonAllocClosest(travelRay, s_RaycastHits, out var closestHitIndex,
				distance, data.CollidesWithLayers, data.TriggerCollision);

			var didHit = numHits > 0;
			hit = didHit ? s_RaycastHits[closestHitIndex] : default;
			return didHit;
		}

		private struct ActiveProjectile
		{
			public Transform Transform;
			public ProjectileData Data;
			public ProjectileRuntimeData RuntimeData;
		}
	}
}
