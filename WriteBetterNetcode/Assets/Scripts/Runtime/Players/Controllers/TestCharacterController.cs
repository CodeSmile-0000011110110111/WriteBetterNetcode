// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile
{
	public sealed class TestCharacterController : ModularCharacterControllerBase
	{
		private void Update()
		{
			m_CharacterController.Move(new Vector3(0f, 0.001f, 0f));
		}
	}
}
