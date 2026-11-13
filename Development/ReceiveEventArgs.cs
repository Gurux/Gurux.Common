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

namespace Gurux.Common
{
    /// <summary>
    /// Argument class for IGXMedia data received events.
    /// </summary>
    public class ReceiveEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes empty receive event data.
        /// </summary>
        public ReceiveEventArgs()
        {

        }

        /// <summary>
        /// Initializes receive event data with a payload and sender information.
        /// </summary>
        /// <param name="data">The received payload.</param>
        /// <param name="senderInfo">Transport-specific sender information.</param>
        public ReceiveEventArgs(object? data, string? senderInfo)
        {
            Data = data;
            SenderInfo = senderInfo;
        }

        /// <summary>
        /// Gets or sets the received payload, typically a byte array.
        /// </summary>
        public object? Data
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets transport-specific sender information.
        /// </summary>
        public string? SenderInfo
        {
            get;
            set;
        }
    }
}
