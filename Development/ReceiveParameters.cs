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
    /// Specifies buffered receive conditions and stores the accumulated reply.
    /// </summary>
    /// <typeparam name="T">Byte array or UTF-8 string reply type supported by the shared receive buffer.</typeparam>
    /// <remarks>
    /// The rules below describe <see cref="GXSynchronousMediaBase"/> and media using its receive implementation.
    /// Set a positive <see cref="Count"/>, a nonempty <see cref="Eop"/>, or <see cref="AllData"/>.
    /// A successful receive returns true. Timeout or closure returns false; cancellation throws
    /// <see cref="OperationCanceledException"/>. False can still accompany a partial reply when AllData is enabled.
    /// Do not modify these parameters or share them with another receive while an operation is pending.
    /// </remarks>
    public class ReceiveParameters<T>
    {        
        /// <summary>
        /// Initializes parameters with peeking disabled and an infinite wait time.
        /// </summary>
        public ReceiveParameters()
        {
            Peek = false;
            WaitTime = -1;
        }
        /// <summary>
        /// Gets or sets whether returned bytes remain available for subsequent receives.
        /// </summary>
        /// <remarks>
        /// Applies to complete packets and partial AllData replies. Returned arrays own their storage.
        /// Peeking still appends to <see cref="Reply"/> and resets <see cref="Count"/> to zero when bytes are returned.
        /// </remarks>
        public bool Peek
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the packet terminator, alternative terminators, or null to receive by byte count.
        /// </summary>
        /// <remarks>
        /// A byte array is one pattern; an object array contains alternative patterns. Each pattern may be
        /// a byte, character, string, byte array, or byte memory. Text is encoded as UTF-8. Empty patterns are invalid.
        /// Matching spans input fragments. The earliest matching end position at or beyond <see cref="Count"/>
        /// completes the packet, including the terminator bytes. Earlier matches do not satisfy the minimum count.
        /// With <see cref="AllData"/>, returned data also includes bytes buffered after that terminator.
        /// </remarks>
        public object? Eop
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the required number of newly received bytes, excluding any existing reply.
        /// </summary>
        /// <remarks>
        /// Without <see cref="Eop"/>, a positive count requests exactly that many bytes unless <see cref="AllData"/>
        /// is enabled. With EOP, count is the minimum packet length, including the terminator.
        /// Zero with AllData and no EOP waits for at least one byte. Negative values are invalid.
        /// Count is reset to zero whenever complete or partial bytes are appended to <see cref="Reply"/>.
        /// </remarks>
        public int Count
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the receive timeout in milliseconds, including time queued behind another reader.
        /// </summary>
        /// <remarks>
        /// The default -1 waits indefinitely; zero polls without waiting. Values below -1 are invalid.
        /// Timeout returns false. Buffered bytes and Reply remain unchanged unless AllData returns partial data
        /// after this operation acquires the reader. Timing out while queued never returns partial data.
        /// </remarks>
        public int WaitTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether a receive returns all currently buffered bytes instead of only the matched packet.
        /// </summary>
        /// <remarks>
        /// When the Count or EOP condition is satisfied, all buffered bytes are returned and the result is true.
        /// On timeout after acquiring the reader, available partial bytes are returned even if the condition is
        /// unsatisfied; the result remains false. Returned bytes are consumed unless <see cref="Peek"/> is true.
        /// Closure does not flush partial bytes. Cancellation throws without returning partial data.
        /// </remarks>
        public bool AllData
        {
            get;
            set;
        }        

        /// <summary>
        /// Gets or sets the accumulated reply, initially the default value of the reply type.
        /// </summary>
        /// <remarks>
        /// Each returned packet or partial AllData result is appended to the previous byte array or string.
        /// New text bytes are decoded as UTF-8 for each receive. Clear Reply to start a separate response.
        /// A timeout without partial data, closure, or cancellation leaves the existing reply unchanged.
        /// Reopening clears the receive buffer but does not clear this parameter object.
        /// </remarks>
        public T? Reply
        {
            get;
            set;
        }        
    }
}
