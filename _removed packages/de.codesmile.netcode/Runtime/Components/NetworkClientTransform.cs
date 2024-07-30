// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode.Components;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	/// Client-authoritative NetworkTransform.
	/// </summary>
	public class NetworkClientTransform : NetworkTransform
	{
		protected override Boolean OnIsServerAuthoritative() => false;
	}
}
