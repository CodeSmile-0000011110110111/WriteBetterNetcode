// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile
{
	public sealed class TestCharacterController : ModularCharacterControllerBase
	{
		private void Update()
		{
			m_CharacterController.Move(new Vector3(0f, 0.0012f, 0f));

			var pos = m_CharacterController.transform.localPosition;
			if (pos.y > 4f)
				m_CharacterController.transform.localPosition = new Vector3(pos.x, 0f, pos.z);
		}
	}
}
