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
    /// An argument class for media state changed event.
    /// </summary>
    public class MediaStateEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes state event data with acceptance enabled.
        /// </summary>
        public MediaStateEventArgs()
        {
            Accept = true;
        }

        /// <summary>
        /// Initializes event data for the specified state.
        /// </summary>
        /// <param name="state">The connection state to report.</param>
        public MediaStateEventArgs(MediaState state)
            : this()
        {
            State = state;
        }

        /// <summary>
        /// Status information from media state.
        /// </summary>
        public MediaState State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the acceptance flag associated with the state change.
        /// Both constructors initialize this flag to true.
        /// </summary>
        public bool Accept
        {
            get;
            set;
        }
    }    
}
