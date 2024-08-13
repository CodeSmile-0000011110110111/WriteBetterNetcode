// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Core.Statemachine;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.BetterNetcode.Netcode
{
	public class AuthenticationState : MonoBehaviour
	{
		public FSM m_Statemachine;

		private void Awake() => SetupStatemachine();

		private void Start()
		{
			m_Statemachine.Start();

			try
			{
				var puml = m_Statemachine.ToPlantUml();
				File.WriteAllText($"{Application.dataPath}/../../PlantUML Diagrams/{GetType().FullName}.puml",
					$"@startuml\n\n!theme blueprint\nhide empty description\n\n{puml}\n\n@enduml");
			}
			catch (Exception e)
			{
				Debug.LogWarning(e);
			}

			m_Statemachine.Update();
		}

		private void SetupStatemachine() => throw new NotImplementedException();
	}
}
