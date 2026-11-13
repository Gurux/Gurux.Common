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
    /// Provides the exception associated with a media error event.
    /// </summary>
    public class MediaErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes error event data with the reported exception.
        /// </summary>
        /// <param name="exception">The exception reported by the media.</param>
        /// <exception cref="ArgumentNullException">The exception is null.</exception>
        public MediaErrorEventArgs(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            Exception = exception;
        }

        /// <summary>
        /// Gets the exception reported by the media.
        /// </summary>
        public Exception Exception { get; }
    }
}
