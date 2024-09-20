﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Samples.Tests
{
	public interface INetworkEventListener
	{
		void OnNetworkEvent(NetworkEventData networkEventData);
	}
}