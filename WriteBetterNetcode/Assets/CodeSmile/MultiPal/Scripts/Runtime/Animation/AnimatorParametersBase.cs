// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	public  class AnimatorParametersBase : INetworkSerializable
	{
		private Byte m_InputMagnitude;
		private Byte m_MoveSpeed;
		private AnimatorCommonFlags m_CommonFlags;

		public Single InputMagnitude
		{
			get => ConvertToFloat(m_InputMagnitude);
			set => m_InputMagnitude = ConvertToByte(value);
		}

		public Single MoveSpeed
		{
			get => ConvertToFloat(m_MoveSpeed);
			set => m_MoveSpeed = ConvertToByte(value);
		}

		public Boolean IsGrounded
		{
			get => IsFlagSet(m_CommonFlags, AnimatorCommonFlags.IsGrounded);
			set => m_CommonFlags = SetOrClearFlag(value, m_CommonFlags, AnimatorCommonFlags.IsGrounded);
		}
		public Boolean IsFalling
		{
			get => IsFlagSet(m_CommonFlags, AnimatorCommonFlags.IsFalling);
			set => m_CommonFlags = SetOrClearFlag(value, m_CommonFlags, AnimatorCommonFlags.IsFalling);
		}

		public Boolean TriggerJump
		{
			get => IsFlagSet(m_CommonFlags, AnimatorCommonFlags.TriggerJump);
			set => m_CommonFlags = SetOrClearFlag(value, m_CommonFlags, AnimatorCommonFlags.TriggerJump);
		}

		public Boolean TriggerCrouch
		{
			get => IsFlagSet(m_CommonFlags, AnimatorCommonFlags.TriggerCrouch);
			set => m_CommonFlags = SetOrClearFlag(value, m_CommonFlags, AnimatorCommonFlags.TriggerCrouch);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Boolean IsFlagSet(AnimatorCommonFlags flags, AnimatorCommonFlags flag) => (flags & flag) != 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static AnimatorCommonFlags SetOrClearFlag(Boolean value, AnimatorCommonFlags flags, AnimatorCommonFlags flag) =>
			value ? flags | flag : flags & ~flag;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Byte ConvertToByte(Single value) => (Byte)Mathf.Min(Byte.MaxValue, value * Byte.MaxValue + 0.5f);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Single ConvertToFloat(Byte value) => value / (Single)Byte.MaxValue;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref m_InputMagnitude);
			serializer.SerializeValue(ref m_MoveSpeed);
			serializer.SerializeValue(ref m_CommonFlags);
		}

		public override String ToString() => $"input:{m_InputMagnitude}, speed:{m_MoveSpeed}, flags:{m_CommonFlags}";
	}

	public enum AnimatorCommonFlags : byte
	{
		IsGrounded = 1 << 0,
		IsFalling = 1 << 1,
		TriggerJump = 1 << 4,
		TriggerCrouch = 1 << 5,
	}
}
