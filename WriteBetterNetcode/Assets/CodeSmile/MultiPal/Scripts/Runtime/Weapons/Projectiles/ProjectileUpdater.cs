// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.CodeSmile.Extensions.UnityEngine;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Weapons.Projectiles
{
	[DisallowMultipleComponent]
	internal sealed class ProjectileUpdater : MonoBehaviour
	{
		private ProjectileSpawner m_Spawner;

		private void Awake()
		{
			m_Spawner = GetComponent<ProjectileSpawner>();
		}

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

			var projectiles = m_Spawner.Projectiles;
			var projectileCount = projectiles.Count;
			for (var i = projectileCount - 1; i >= 0; i--)
			{
				var projectile = projectiles[i];
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
					var layerMask = projectile.Data.CollidesWithLayers;
					var triggerInteraction = projectile.Data.TriggerInteraction;
					if (PhysicsExt.ClosestHit(travelRay, out hit, distanceTravelled, layerMask, triggerInteraction))
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
					m_Spawner.DestroyProjectile(projectile);
					projectiles.RemoveAt(i);
				}
			}
		}
	}
}
