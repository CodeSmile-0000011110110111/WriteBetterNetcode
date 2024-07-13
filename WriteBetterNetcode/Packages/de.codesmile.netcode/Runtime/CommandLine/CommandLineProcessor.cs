// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;
using UnityEngine;

namespace CodeSmile.Netcode.CommandLine
{
	public class CommandLineProcessor : OneTimeTaskBehaviour
	{
		[SerializeField] private CmdArgNetStart m_NetStart;
		[SerializeField] private CmdArgRelay m_Relay;
		[SerializeField] private CmdArgAddress m_Address;
		[SerializeField] private CmdArgPort m_Port;

		private ICmdArgImpl[] m_Arguments;

		private void OnValidate()
		{
			if (m_Arguments == null || m_Arguments.Length == 0)
				m_Arguments = CreateArgumentList();

			foreach (var argument in m_Arguments)
				argument.OnValidate();
		}

		private void Reset()
		{
			m_Arguments = CreateArgumentList();

			foreach (var argument in m_Arguments)
				argument.Reset();
		}

		private void Start()
		{
			if (Application.isEditor)
			{
				TaskPerformed();
				return;
			}

			m_Arguments = CreateArgumentList();

			foreach (var argument in m_Arguments)
				argument.Process();

			TaskPerformed();
		}

		// Order may be important!
		private ICmdArgImpl[] CreateArgumentList() => new ICmdArgImpl[]
		{
			m_Address,
			m_Port,
			m_Relay,
			m_NetStart,
		};
	}
}
