// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.MultiPal.Animation
{
	public class AvatarAnimatorParameters
	{
		private const Int32 MinParameterCount = 3;

		private const Int32 InputMagnitudeIndex = 0;
		private const Int32 MoveSpeedIndex = 1;
		private const Int32 CommonFlagsIndex = 2;

		protected Byte[] m_Parameters;

		public AvatarAnimatorParameters(int parameterCount = MinParameterCount)
		{
			if (parameterCount < MinParameterCount)
				throw new ArgumentException($"at least {MinParameterCount} parameters required");

			m_Parameters = new byte[parameterCount];
		}

		public Byte[] Parameters
		{
			get => m_Parameters;
			set => m_Parameters = value;
		}

		public Single InputMagnitude
		{
			get => ConvertToFloat(m_Parameters[InputMagnitudeIndex]);
			set => m_Parameters[InputMagnitudeIndex] = ConvertToByte(value);
		}

		public Single MoveSpeed
		{
			get => ConvertToFloat(m_Parameters[MoveSpeedIndex]);
			set => m_Parameters[MoveSpeedIndex] = ConvertToByte(value);
		}

		public Boolean IsGrounded
		{
			get => IsFlagSet(m_Parameters[CommonFlagsIndex], (Byte)AnimatorCommonFlags.IsGrounded);
			set => m_Parameters[CommonFlagsIndex] =
				(Byte)SetOrClearFlag(value, m_Parameters[CommonFlagsIndex], (Byte)AnimatorCommonFlags.IsGrounded);
		}
		public Boolean IsFalling
		{
			get => IsFlagSet(m_Parameters[CommonFlagsIndex], (Byte)AnimatorCommonFlags.IsFalling);
			set => m_Parameters[CommonFlagsIndex] =
				(Byte)SetOrClearFlag(value, m_Parameters[CommonFlagsIndex], (Byte)AnimatorCommonFlags.IsFalling);
		}

		public Boolean TriggerJump
		{
			get => IsFlagSet(m_Parameters[CommonFlagsIndex], (Byte)AnimatorCommonFlags.TriggerJump);
			set => m_Parameters[CommonFlagsIndex] =
				(Byte)SetOrClearFlag(value, m_Parameters[CommonFlagsIndex], (Byte)AnimatorCommonFlags.TriggerJump);
		}

		public Boolean TriggerCrouch
		{
			get => IsFlagSet(m_Parameters[CommonFlagsIndex], (Byte)AnimatorCommonFlags.TriggerCrouch);
			set => m_Parameters[CommonFlagsIndex] =
				(Byte)SetOrClearFlag(value, m_Parameters[CommonFlagsIndex], (Byte)AnimatorCommonFlags.TriggerCrouch);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Boolean IsFlagSet(Int32 flags, Int32 flag) => (flags & flag) != 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Int32 SetOrClearFlag(Boolean value, Int32 flags, Int32 flag) => value ? flags | flag : flags & ~flag;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Byte ConvertToByte(Single value) => (Byte)Mathf.Min(Byte.MaxValue, value * Byte.MaxValue + 0.5f);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Single ConvertToFloat(Byte value) => value / (Single)Byte.MaxValue;
	}

	public enum AnimatorCommonFlags : byte
	{
		IsGrounded = 1 << 0,
		IsFalling = 1 << 1,
		IsDying = 1 << 2,
		// tbd

		TriggerJump = 1 << 4,
		TriggerCrouch = 1 << 5,
		TriggerAttack = 1 << 6,
		TriggerInteract = 1 << 7,
	}
}
