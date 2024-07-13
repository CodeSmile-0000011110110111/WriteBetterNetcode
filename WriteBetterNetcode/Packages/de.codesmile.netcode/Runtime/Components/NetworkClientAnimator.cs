// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode.Components;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	/// Client-authoritative NetworkAnimator.
	/// </summary>
	public class NetworkClientAnimator : NetworkAnimator
	{
		protected override Boolean OnIsServerAuthoritative() => false;
	}
}
