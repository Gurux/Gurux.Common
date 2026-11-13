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
    /// This class is used to save command line parameters.
    /// </summary>
    public class GXCmdParameter
    {
        /// <summary>
        /// Command line parameter tag
        /// </summary>
        public char Tag
        {
            get;
            internal set;
        }

        /// <summary>
        /// Command line parameter value.
        /// </summary>
        public string? Value
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets whether the parser has marked a required option value as missing.
        /// </summary>
        public bool Missing
        {
            get;
            internal set;
        }

        /// <summary>
        /// Command line parameter as a string.
        /// </summary>
        /// <returns>The option tag, a tag=value pair, or a missing-value message.</returns>
        public override string ToString()
        {
            if (Missing)
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.MissingParameterValue, Tag);
            }
            if (Value == null)
            {
                return Tag.ToString();
            }
            return Tag + "=" + Value;
        }
    }
}
