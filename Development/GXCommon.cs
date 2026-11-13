// --------------------------------------------------------------------------
// Gurux Ltd
//
// Copyright (c) Gurux Ltd.
//
// This file is part of Gurux Device Framework.
//
// Commit: $Format:%H$
// Date:   $Format:%cI$
//
// This software is available under either of the following licenses:
//
// 1. GNU General Public License, version 2 only (GPL-2.0-only).
//    See the LICENSE file included with this distribution.
//
// 2. A commercial license granted by Gurux Ltd under a separate written
//    agreement. Use under this option requires a valid commercial license
//    and is governed by the terms of that agreement.
//
// If you have not obtained a commercial license from Gurux Ltd,
// use of this software is governed by GPL-2.0-only.
//
// Commercial licensing: https://www.gurux.fi/
//
// SPDX-License-Identifier: GPL-2.0-only OR LicenseRef-Gurux-Commercial
// --------------------------------------------------------------------------

using System.Diagnostics;

namespace Gurux.Common
{
    /// <summary>
    /// Provides hexadecimal conversion and command-line parsing helpers.
    /// </summary>
    public static class GXCommon
    {
        /// <summary>
        /// Converts bytes to an uppercase hex string.
        /// </summary>
        /// <param name="bytes">Byte array to convert. Null produces an empty string.</param>
        /// <param name="addSpace">Whether to separate bytes with spaces.</param>
        /// <param name="index">Index of the first byte to convert.</param>
        /// <param name="count">Number of bytes to convert, or null for all remaining bytes.</param>
        /// <returns>The uppercase hex string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The array is not null and the requested range is outside its bounds.
        /// </exception>
        /// <remarks>
        /// Each byte produces two hexadecimal digits. Spaces, when requested, appear only between bytes.
        /// A null array returns an empty string without validating the range.
        /// </remarks>
        /// <example>
        /// <code>
        /// GXCommon.ToHex(new byte[] { 0x01, 0xAB, 0xFF }); // "01 AB FF"
        /// GXCommon.ToHex(new byte[] { 0x01, 0xAB, 0xFF }, false); // "01ABFF"
        /// </code>
        /// </example>
        [DebuggerStepThrough]
        public static string ToHex(byte[]? bytes, bool addSpace = true, int index = 0, int? count = null)
        {
            if (bytes == null)
            {
                return string.Empty;
            }
            ReadOnlySpan<byte> data = bytes.AsSpan(index, count ?? (bytes.Length - index));
            if (data.IsEmpty)
            {
                return string.Empty;
            }
            return addSpace
                ? BitConverter.ToString(bytes, index, data.Length).Replace('-', ' ')
                : Convert.ToHexString(data);
        }

        /// <summary>
        /// Converts a hexadecimal string to bytes, ignoring spaces and hyphens.
        /// </summary>
        /// <param name="value">Hexadecimal text containing uppercase or lowercase digits.</param>
        /// <returns>The decoded bytes, or an empty array if the input is null or empty.</returns>
        /// <exception cref="FormatException">
        /// After spaces and hyphens are removed, the input has an odd length or contains a non-hexadecimal character.
        /// </exception>
        /// <remarks>
        /// Only ordinary spaces and hyphens are removed. Tabs, line breaks, and hexadecimal prefixes are not accepted.
        /// </remarks>
        public static byte[] HexToBytes(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return [];
            }
            return Convert.FromHexString(value.Replace(" ", "").Replace("-", ""));
        }

        /// <summary>
        /// Parses single-character command-line options and their values.
        /// </summary>
        /// <param name="args">Options prefixed with '-' or '/', followed by separate value arguments where required.</param>
        /// <param name="optstring">Recognized option characters. A colon after a character indicates a required value.</param>
        /// <returns>Parsed options in input order.</returns>
        /// <exception cref="ArgumentException">An option has an invalid prefix, length, or option character.</exception>
        /// <exception cref="ArgumentNullException">An input argument is null.</exception>
        /// <remarks>Options must have exactly one character after the prefix. A missing trailing value is reported using GXCmdParameter.Missing.</remarks>
        /// <example>
        /// <code>
        /// var options = GXCommon.GetParameters(new[] { "-p", "COM1", "-v" }, "p:v");
        /// </code>
        /// </example>
        public static List<GXCmdParameter> GetParameters(string[] args, string optstring)
        {
            ArgumentNullException.ThrowIfNull(args);
            ArgumentNullException.ThrowIfNull(optstring);
            List<GXCmdParameter> list = new List<GXCmdParameter>();
            for (int index = 0; index < args.Length; ++index)
            {
                if (args[index] == null || args[index].Length != 2 ||
                    (args[index][0] != '-' && args[index][0] != '/'))
                {
                    throw new ArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.InvalidParameter, args[index]));
                }
                int pos = optstring.IndexOf(args[index][1]);
                if (pos == -1 || args[index][1] == ':')
                {
                    throw new ArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.InvalidParameter, args[index]));
                }
                GXCmdParameter c = new GXCmdParameter();
                c.Tag = args[index][1];
                list.Add(c);
                if (pos < optstring.Length - 1 && optstring[1 + pos] == ':')
                {
                    ++index;
                    if (args.Length <= index)
                    {
                        c.Missing = true;
                        break;
                    }
                    c.Value = args[index];
                }
            }
            return list;
        }
    }
}
