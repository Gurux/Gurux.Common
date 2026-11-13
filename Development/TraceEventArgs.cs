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

using System;
using System.Text;

namespace Gurux.Common
{
    /// <summary>
    /// Provides the timestamp, category, and payload of a media trace event.
    /// </summary>
    public class TraceEventArgs : EventArgs
    {
        /// <summary>
        /// Timestamp of the event.
        /// </summary>
        public DateTime Timestamp
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the trace category, such as sent data, received data, or an error.
        /// </summary>
        public TraceTypes Type
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the payload associated with the trace event.
        /// </summary>
        public object? Data
        {
            get;
            internal set;
        }

        /// <summary>
        /// Data receiver.
        /// </summary>
        public string? Receiver
        {
            get;
            set;
        }

        /// <summary>
        /// Convert data to string.
        /// </summary>
        /// <param name="ascii">Is content get as ascii or hex string.</param>
        /// <returns>Content of data as string.</returns>
        public string? DataToString(bool ascii)
        {
            if (Data == null)
            {
                return "";
            }
            if (Data is byte[] data)
            {
                if (ascii)
                {
                    return ASCIIEncoding.ASCII.GetString(data);
                }
                return GXCommon.ToHex(data, true);
            }
            return Convert.ToString(Data);
        }

        /// <summary>
        /// Formats the timestamp, category, and data as a tab-separated trace entry.
        /// </summary>
        /// <returns>The formatted trace entry.</returns>
        public override string ToString()
        {
            return Timestamp.ToString("hh:mm:ss.fff") + "\t" + Type.ToString() + "\t" + DataToString(false);
        }

        /// <summary>
        /// Initializes a trace event using the current local time.
        /// </summary>
        /// <param name="type">The trace category.</param>
        /// <param name="data">The event payload.</param>
        /// <param name="receiver">Transport-specific destination information.</param>
        public TraceEventArgs(TraceTypes type, object? data, string? receiver)
        {
            Timestamp = DateTime.Now;
            Type = type;
            Data = data;
            Receiver = receiver;
        }

        /// <summary>
        /// Initializes a trace event by copying a byte range at the current local time.
        /// </summary>
        /// <param name="type">The trace category.</param>
        /// <param name="data">The source buffer.</param>
        /// <param name="index">The first byte to copy.</param>
        /// <param name="length">The number of bytes to copy.</param>
        /// <param name="receiver">Transport-specific destination information.</param>
        public TraceEventArgs(TraceTypes type, byte[] data, int index, int length, string? receiver)
        {
            Timestamp = DateTime.Now;
            Type = type;
            byte[] tmp = new byte[length];
            Array.Copy(data, index, tmp, 0, length);
            Data = tmp;
            Receiver = receiver;
        }
    }
}
