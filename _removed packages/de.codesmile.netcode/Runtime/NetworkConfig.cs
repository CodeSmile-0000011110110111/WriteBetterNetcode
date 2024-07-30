// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

namespace CodeSmile.Netcode
{
	public class NetworkConfig
	{
		/// <summary>
		/// Authentication session token.
		/// <remarks>
		/// Token is also available via PlayerPrefs key: $CloudProjectId.$Profile.unity.services.authentication.session_token
		/// </remarks>
		/// </summary>
		public string SessionToken { get; set; } = "";
	}
}
