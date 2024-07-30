﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile
{
	public static class PhysicsExt
	{
		/// <summary>
		///     Returns the index of the closest (least distant) hit in the results array. Uses RaycastNonAlloc.
		/// </summary>
		/// <param name="ray">The ray to cast.</param>
		/// <param name="results">The pre-allocated array where results are stored.</param>
		/// <param name="closestHitIndex">Index in results of the closest hit. Is -1 if there are no hits.</param>
		/// <param name="maxDistance">Maximum raycast distance. Default: infinity.</param>
		/// <param name="layerMask">Layer mask. Default: Physics.DefaultRaycastLayers</param>
		/// <param name="triggerInteraction">How to interact with trigger colliders. Default: QueryTriggerInteraction.UseGlobal</param>
		/// <returns>The number of hits.</returns>
		public static Int32 RaycastNonAllocClosest(Ray ray, RaycastHit[] results, out Int32 closestHitIndex,
			Single maxDistance = Mathf.Infinity, Int32 layerMask = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			closestHitIndex = -1;
			var numHits = Physics.RaycastNonAlloc(ray, results, maxDistance, layerMask, triggerInteraction);
			if (numHits != 0)
			{
				closestHitIndex = 0;
				for (var i = 1; i < numHits; i++)
					if (results[i].distance < results[closestHitIndex].distance)
						closestHitIndex = i;
			}

			return numHits;
		}
	}
}
