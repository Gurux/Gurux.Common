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

namespace Gurux.Common;

/// <summary>Optional UI adapter; core media libraries have no UI framework dependency.</summary>
public interface IGXMediaSettingsEditor
{
    /// <summary>Current editor window, or null when no view exists.</summary>
    IGXWindow? Window { get; }
    /// <summary>Shows staged settings; applies changes only when accepted.</summary>
    /// <param name="media">Media whose settings are staged and updated on acceptance.</param>
    /// <param name="parent">Optional parent window.</param>
    /// <param name="cancellationToken">Token requesting cancellation of the editor.</param>
    /// <returns>Whether the user accepted the staged settings.</returns>
    /// <exception cref="OperationCanceledException">The editor operation is canceled.</exception>
    Task<bool> ShowAsync(IGXMedia media, IGXWindow? parent = null, CancellationToken cancellationToken = default);
}
