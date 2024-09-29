// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Settings;
using CodeSmile.MultiPal.Weapons;
using CodeSmile.MultiPal.Weapons.Projectiles;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerWeaponNetcode))]
	public sealed class PlayerWeapon : MonoBehaviour, IPlayerComponent
	{
		[SerializeField] private WeaponPrefabs m_WeaponPrefabs;

		private Int32 m_ActiveWeaponIndex;
		private ProjectileSpawner m_ProjectileSpawner;
		private PlayerAvatar m_PlayerAvatar;
		private PlayerWeaponNetcode m_Netcode;
		private Weapon m_ActiveWeapon;

		private Int32 m_PlayerIndex;
		private bool m_IsOwner;

		public void OnPlayerSpawn(Int32 playerIndex, Boolean isOwner)
		{
			if (isOwner)
			{
				m_PlayerIndex = playerIndex;
				m_IsOwner = isOwner;

				StartCoroutine(TestCycleWeapon());
				StartCoroutine(TestFireWeapon());
			}
		}

		public void OnPlayerDespawn(Int32 playerIndex, Boolean isOwner) => m_ActiveWeapon = null;

		private void Awake()
		{
			m_PlayerAvatar = GetComponent<PlayerAvatar>();
			m_Netcode = GetComponent<PlayerWeaponNetcode>();
		}

		private void Start() => m_ProjectileSpawner = ComponentsRegistry.Get<ProjectileSpawner>();

		private IEnumerator TestFireWeapon()
		{
			while (true)
			{
				m_Netcode.StartAttacking();
				if (m_IsOwner)
					StartAttacking();

				yield return new WaitForSeconds(3f);

				m_Netcode.StopAttacking();
				if (m_IsOwner)
					StopAttacking();

				yield return new WaitForSeconds(1.2f);
			}
		}

		private IEnumerator TestCycleWeapon()
		{
			while (true)
			{
				NextWeapon();
				yield return new WaitForSeconds(15f);
			}
		}

		private void NextWeapon()
		{
			m_ActiveWeaponIndex++;
			if (m_ActiveWeaponIndex >= m_WeaponPrefabs.Count)
				m_ActiveWeaponIndex = 0;

			var weaponPrefab = m_WeaponPrefabs[m_ActiveWeaponIndex];
			m_ActiveWeapon = m_PlayerAvatar.SetWeapon(weaponPrefab);

			if (m_ActiveWeapon == null)
				Debug.LogWarning($"Weapon {weaponPrefab} has no Weapon component");
		}

		internal void StartAttacking()
		{
			m_ActiveWeapon.StartAttacking();
		}
		internal void StopAttacking()
		{
			m_ActiveWeapon.StopAttacking();
		}
	}
}
