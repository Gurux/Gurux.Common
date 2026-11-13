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
    /// Argument class for IGXMedia connection and disconnection events.
    /// </summary>
    public class ConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes connection event data with acceptance enabled.
        /// </summary>
        public ConnectionEventArgs()
        {
            Accept = true;
        }

        /// <summary>
        /// Initializes connection event data with acceptance enabled.
        /// </summary>
        /// <param name="info">Transport-specific connection information.</param>
        public ConnectionEventArgs(string? info)
        {
            Accept = true;
            Info = info;
        }

        /// <summary>
        /// Gets or sets transport-specific connection information.
        /// </summary>
        public string? Info
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the client connection is accepted. The default is true.
        /// </summary>
        public bool Accept
        {
            get;
            set;
        }
    }
}
