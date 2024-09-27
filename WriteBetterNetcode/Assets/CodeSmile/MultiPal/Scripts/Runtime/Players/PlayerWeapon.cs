// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Settings;
using CodeSmile.MultiPal.Weapons;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Players
{
	[DisallowMultipleComponent]
	public sealed class PlayerWeapon : MonoBehaviour, IPlayerComponent
	{
		[SerializeField] private WeaponPrefabs m_WeaponPrefabs;

		private Int32 m_ActiveWeaponIndex;
		private Projectiles m_Projectiles;
		private PlayerAvatar m_PlayerAvatar;
		private PlayerClient m_ClientSide;
		private Weapon m_ActiveWeapon;

		private Int32 m_PlayerIndex;

		public void OnPlayerSpawn(Int32 playerIndex, Boolean isOwner)
		{
			if (isOwner)
			{
				m_PlayerIndex = playerIndex;

				StartCoroutine(TestCycleWeapon());
				StartCoroutine(TestFireWeapon());
			}
		}

		public void OnPlayerDespawn(Int32 playerIndex, Boolean isOwner) => m_ActiveWeapon = null;

		private void Awake()
		{
			m_PlayerAvatar = GetComponent<PlayerAvatar>();
			m_ClientSide = GetComponent<PlayerClient>();
		}

		private void Start() => m_Projectiles = ComponentsRegistry.Get<Projectiles>();

		private IEnumerator TestFireWeapon()
		{
			while (true)
			{
				m_ClientSide.StartAttacking();

				yield return new WaitForSeconds(1f);

				m_ClientSide.StopAttacking();

				yield return new WaitForSeconds(.25f);
			}
		}

		private IEnumerator TestCycleWeapon()
		{
			while (true)
			{
				NextWeapon();
				yield return new WaitForSeconds(2f);
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

		private void StartAttacking()
		{
			throw new NotImplementedException();

		}

		private void StopAttacking()
		{
			throw new NotImplementedException();

		}

		private void FireWeapon()
		{
			if (m_ActiveWeapon != null)
			{
				m_ActiveWeapon.FireAudioVisual();

				var weaponData = m_ActiveWeapon.DataAsset.Data;
				m_Projectiles.FireWeapon(weaponData, m_ActiveWeapon.ProjectileSpawnPoints);
			}
		}
	}
}
