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
using System.Runtime.Serialization;

namespace Gurux.Common
{
    /// <summary>
    /// Identifies trace message categories. Values can be combined as flags.
    /// </summary>
    [Flags]
    public enum TraceTypes
    {
        /// <summary>
        /// Data is sent.
        /// </summary>
        [EnumMember(Value = "1")]
        Sent = 0x1,
        /// <summary>
        /// Data is received.
        /// </summary>
        [EnumMember(Value = "2")]
        Received = 0x2,
        /// <summary>
        /// Error has occurred.
        /// </summary>
        [EnumMember(Value = "4")]
        Error = 0x4,
        /// <summary>
        /// Warning.
        /// </summary>
        [EnumMember(Value = "8")]
        Warning = 0x8,
        /// <summary>
        /// Informational messages, including connection state notifications.
        /// </summary>
        [EnumMember(Value = "16")]
        Info = 0x10,
    }
}
