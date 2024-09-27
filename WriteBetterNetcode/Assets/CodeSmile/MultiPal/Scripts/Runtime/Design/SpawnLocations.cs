// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeSmile.MultiPal.Design
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public sealed partial class SpawnLocations : MonoBehaviour
	{
		public static event Action<SpawnLocations> OnSpawnLocationsChanged;
		private static List<SpawnLocation> m_AllSpawnLocations;

		//[HideInInspector]
		[SerializeField] private SpawnLocation[] m_SpawnLocations = new SpawnLocation[0];

		public static Int32 Count => m_AllSpawnLocations.Count;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticFields()
		{
			m_AllSpawnLocations = new();
			OnSpawnLocationsChanged = null;
		}

		private void Awake()
		{
			// since we execute in edit mode
			if (Application.isPlaying)
			{
				AddToStaticLocations();
				ComponentsRegistry.Set(this);
			}
		}

		private void OnDestroy()
		{
			// since we execute in edit mode
			if (Application.isPlaying)
				RemoveFromStaticLocations();
		}

		private void OnEnable()
		{
			if (Application.isPlaying) {}
			else
			{
				RegisterEditorSceneEvents();
				UpdateSpawnLocationsList(gameObject.scene);
			}
		}

		private void AddToStaticLocations()
		{
			m_AllSpawnLocations.AddRange(m_SpawnLocations);
			OnSpawnLocationsChanged?.Invoke(this);
		}

		private void RemoveFromStaticLocations()
		{
			foreach (var spawnLocation in m_SpawnLocations)
				m_AllSpawnLocations.Remove(spawnLocation);
			OnSpawnLocationsChanged?.Invoke(this);
		}

		public SpawnLocation GetRandomSpawnLocation(Int32 playerIndex)
		{
			if (m_AllSpawnLocations.Count == 0)
			{
				Debug.LogWarning("No SpawnLocations available.");
				return null;
			}

			const Int32 MaxIterations = 250; // infinite loop safeguard
			var iterations = 0;
			SpawnLocation location;
			do
			{
				var randomIndex = Random.Range(0, m_AllSpawnLocations.Count);
				location = m_AllSpawnLocations[randomIndex];

				if (++iterations < MaxIterations)
					break;
			} while (location == null || location.IsPlayerAllowed(playerIndex) == false);

			return location;
		}
	}
}
