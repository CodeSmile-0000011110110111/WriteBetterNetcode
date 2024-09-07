// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.Settings;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Input
{
	public sealed partial class InputUsers
	{
		private readonly GeneratedInput[] m_GeneratedInputs = new GeneratedInput[Constants.MaxCouchPlayers];
		public Boolean AllPairingEnabled
		{
			set
			{
				foreach (var input in m_GeneratedInputs)
				{
					var pairing = input.Pairing;
					if (value)
					{
						pairing.Enable();
						pairing.Join.Enable();
						pairing.Leave.Disable(); // leave is enabled only after joining
					}
					else
					{
						pairing.Disable();
						pairing.Join.Disable();
						pairing.Leave.Disable();
					}
				}
			}
		}
		public Boolean AllUiEnabled
		{
			set
			{
				foreach (var input in m_GeneratedInputs)
				{
					if (value)
						input.UI.Enable();
					else
						input.UI.Disable();
				}
			}
		}
		public Boolean AllPlayerKinematicsEnabled
		{
			set
			{
				foreach (var input in m_GeneratedInputs)
				{
					if (value)
						input.PlayerKinematics.Enable();
					else
						input.PlayerKinematics.Disable();
				}
			}
		}

		public Boolean AllPlayerInteractionEnabled
		{
			set
			{
				foreach (var input in m_GeneratedInputs)
				{
					if (value)
						input.PlayerInteraction.Enable();
					else
						input.PlayerInteraction.Disable();
				}
			}
		}

		public Boolean AllPlayerUiEnabled
		{
			set
			{
				// enable or disable everyone's Player inputs
				foreach (var input in m_GeneratedInputs)
				{
					if (value)
						input.PlayerUI.Enable();
					else
						input.PlayerUI.Disable();
				}
			}
		}

		public void SetUiCallback(Int32 playerIndex, GeneratedInput.IUIActions callback) =>
			m_GeneratedInputs[playerIndex].UI.SetCallbacks(callback);

		public void SetUiEnabled(Int32 playerIndex, Boolean enable)
		{
			var ui = m_GeneratedInputs[playerIndex].UI;
			if (enable)
				ui.Enable();
			else
				ui.Disable();

			//LogActionEnabledness($"{playerIndex} => UI {(enable ? "enable" : "disable")}\n");
		}

		public void SetPlayerKinematicsCallback(Int32 playerIndex, GeneratedInput.IPlayerKinematicsActions callback) =>
			m_GeneratedInputs[playerIndex].PlayerKinematics.SetCallbacks(callback);

		public void SetPlayerKinematicsEnabled(Int32 playerIndex, Boolean enable)
		{
			var playerKinematics = m_GeneratedInputs[playerIndex].PlayerKinematics;
			if (enable)
				playerKinematics.Enable();
			else
				playerKinematics.Disable();

			//LogActionEnabledness($"{playerIndex} => Kinematics {(enable ? "enable" : "disable")}\n");
		}

		public void SetPlayerInteractionCallback(Int32 playerIndex, GeneratedInput.IPlayerInteractionActions callback) =>
			m_GeneratedInputs[playerIndex].PlayerInteraction.SetCallbacks(callback);

		public void SetPlayerInteractionEnabled(Int32 playerIndex, Boolean enable)
		{
			var playerInteraction = m_GeneratedInputs[playerIndex].PlayerInteraction;
			if (enable)
				playerInteraction.Enable();
			else
				playerInteraction.Disable();

			//LogActionEnabledness($"{playerIndex} => Interaction {(enable ? "enable" : "disable")}\n");
		}

		public void SetPlayerUiCallback(Int32 playerIndex, GeneratedInput.IPlayerUIActions callback) =>
			m_GeneratedInputs[playerIndex].PlayerUI.SetCallbacks(callback);

		public void SetPlayerUiEnabled(Int32 playerIndex, Boolean enable)
		{
			var playerUi = m_GeneratedInputs[playerIndex].PlayerUI;
			if (enable)
				playerUi.Enable();
			else
				playerUi.Disable();

			//LogActionEnabledness($"{playerIndex} => PlayerUI {(enable ? "enable" : "disable")}\n");
		}

		public void SetPlayerUiRequestMenuEnabled(Int32 playerIndex, Boolean enable)
		{
			var requestMenuAction = m_GeneratedInputs[playerIndex].PlayerUI.RequestMenu;
			if (enable)
				requestMenuAction.Enable();
			else
				requestMenuAction.Disable();
		}

		public void LogActionEnabledness(String prefix)
		{
			var sb = new StringBuilder(prefix);
			for (var i = 0; i < m_GeneratedInputs.Length; i++)
			{
				var input = m_GeneratedInputs[i];
				var move = input.PlayerKinematics.Move;
				var kinematics = input.PlayerKinematics.enabled ? "ON" : "--";
				var interaction = input.PlayerInteraction.enabled ? "ON" : "--";
				var playerUi = input.PlayerUI.enabled ? "ON" : "--";
				var ui = input.UI.enabled ? "ON" : "--";
				var pairing = input.Pairing.enabled ? "ON" : "--";
				sb.AppendLine($"[{i}] Kinematics: {kinematics} Interaction: {interaction} " +
				              $"PlayerUI: {playerUi} UI: {ui} Pairing: {pairing}");
			}

			Debug.Log(sb.ToString());
		}
	}
}
