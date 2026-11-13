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
    /// Extends <see cref="IGXMedia"/> with asynchronous connection operations.
    /// </summary>
    /// <remarks>
    /// Implementations must serialize connection lifecycle operations and reject a second concurrent open
    /// with InvalidOperationException. Opening an already open media is a no-op.
    /// Closing cancels an in-progress open, waits for its cleanup, and releases connection resources.
    /// Repeated or concurrent close calls must safely complete with the connection closed.
    /// Raise Opening before acquiring the connection and Open before completing a successful open task.
    /// An unsuccessful open returns to Closed after cleanup. Raise Closing and then Closed during shutdown.
    /// Do not hold internal lifecycle locks while invoking event handlers.
    /// Await the returned tasks to observe completion and connection errors.
    /// </remarks>
    public interface IGXAsyncMedia : IGXMedia
    {
        /// <summary>
        /// Opens the media asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token used to request cancellation of the connection attempt.</param>
        /// <returns>A task that completes when the connection is ready for use.</returns>
        /// <exception cref="OperationCanceledException">The connection attempt is canceled.</exception>
        /// <remarks>
        /// Cancellation support and connection errors depend on the transport.
        /// Implementations should release resources acquired by an unsuccessful connection attempt.
        /// </remarks>
        Task OpenAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Closes the media asynchronously and releases connection resources.
        /// </summary>
        /// <returns>A task that completes when connection shutdown has finished.</returns>
        /// <remarks>
        /// Implementations should finish their connection-related background work before completing the task.
        /// </remarks>
        Task CloseAsync();

        /// <summary>Sends bytes asynchronously to the default or specified destination.</summary>
        /// <param name="data">The bytes to send. Keep the memory valid until completion.</param>
        /// <param name="receiver">Transport-specific destination, or null for the default.</param>
        /// <param name="cancellationToken">Cancels the pending send.</param>
        /// <returns>An operation that completes after the bytes are transmitted.</returns>
        ValueTask SendAsync(ReadOnlyMemory<byte> data, string? receiver = null, CancellationToken cancellationToken = default);

        /// <summary>Enables buffered receives across awaits. Dispose after the request/reply exchange.</summary>
        /// <returns>A scope that releases buffered receive mode on disposal.</returns>
        IDisposable BeginSynchronous();

        /// <summary>Receives a buffered packet asynchronously.</summary>
        /// <remarks>
        /// Enable buffered mode with <see cref="BeginSynchronous"/> before sending the request.
        /// Packet matching, partial replies and peeking follow <see cref="ReceiveParameters{T}"/>.
        /// Timeout preserves unread bytes unless AllData returns partial data; cancellation preserves them.
        /// Closing wakes pending receives with false without flushing partial data. Readers from an older
        /// connection must not consume bytes received after reopening. Transport failures fault the task.
        /// </remarks>
        /// <typeparam name="T">Byte array or UTF-8 string reply type.</typeparam>
        /// <param name="args">Receive conditions and accumulated reply.</param>
        /// <param name="cancellationToken">Cancels this receive without consuming unread bytes.</param>
        /// <returns>A task returning true when the conditions are satisfied, or false on timeout or closure.
        /// With AllData, false on timeout may still append a partial reply.</returns>
        /// <exception cref="ArgumentNullException">The receive parameters are null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Count is negative or WaitTime is below -1.</exception>
        /// <exception cref="ArgumentException">No receive condition is specified or an EOP pattern is invalid.</exception>
        /// <exception cref="NotSupportedException">The reply type is unsupported.</exception>
        /// <exception cref="InvalidOperationException">The media is closed when the receive is started.</exception>
        /// <exception cref="OperationCanceledException">The receive is canceled.</exception>
        Task<bool> ReceiveAsync<T>(ReceiveParameters<T> args, CancellationToken cancellationToken = default);
    }
}
