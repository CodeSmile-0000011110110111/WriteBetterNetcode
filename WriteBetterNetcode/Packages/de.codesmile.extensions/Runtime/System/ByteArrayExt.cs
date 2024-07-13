// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Text;

namespace CodeSmile
{
	public static class ByteArrayExt
	{
		/// <summary>
		///     Converts the byte array to a UTF8 string. Mainly used to debug print the byte[] contents.
		/// </summary>
		/// <param name="bytes">Input array</param>
		/// <param name="maxLength">How many bytes to convert to string. If negative, entire array is converted.</param>
		/// <param name="encoding">The string encoding to use. Defaults to ASCII.</param>
		/// <returns>The byte array as string. If the array is empty returns an empty string.</returns>
		public static String GetString(this Byte[] bytes, Int32 maxLength = -1, Encoding encoding = null)
		{
			var count = maxLength < 0 ? bytes.Length : Math.Min(bytes.Length, maxLength);
			return encoding switch
			{
				ASCIIEncoding asciiEncoding => asciiEncoding.GetString(bytes, 0, count),
				UnicodeEncoding unicodeEncoding => unicodeEncoding.GetString(bytes, 0, count),
				UTF32Encoding utf32Encoding => utf32Encoding.GetString(bytes, 0, count),
				UTF7Encoding utf7Encoding => utf7Encoding.GetString(bytes, 0, count),
				UTF8Encoding utf8Encoding => utf8Encoding.GetString(bytes, 0, count),
				null => Encoding.ASCII.GetString(bytes, 0, count),
				_ => Encoding.ASCII.GetString(bytes, 0, count),
			};
		}
	}
}
