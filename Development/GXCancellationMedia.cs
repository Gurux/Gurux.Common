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

using System.Diagnostics;

namespace Gurux.Common
{
    /// <summary>Links application cancellation to every transport operation, including readout helpers without token parameters.</summary>
    /// <param name="inner">Caller-owned transport to receive forwarded operations.</param>
    /// <param name="applicationToken">Token that cancels communication when the application is interrupted.</param>
    /// <exception cref="ArgumentNullException">The wrapped transport is null.</exception>
    /// <remarks>Wrapping the transport does not transfer ownership of its resources.</remarks>
    internal sealed class GXCancellationMedia(IGXAsyncMedia inner, CancellationToken applicationToken) : IGXAsyncMedia
    {
        /// <summary>Transport receiving forwarded operations and retaining ownership of its resources.</summary>
        private readonly IGXAsyncMedia inner = inner ?? throw new ArgumentNullException(nameof(inner));
        /// <summary>Application lifetime token linked to transport opens, sends and receives.</summary>
        private readonly CancellationToken applicationToken = applicationToken;

        /// <inheritdoc/>
        /// <remarks>Application cancellation is observed in addition to any operation cancellation token.</remarks>
        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(applicationToken, cancellationToken);
            linked.Token.ThrowIfCancellationRequested();
            if (inner.IsOpen)
            {
                return;
            }

            try
            {
                await inner.OpenAsync(linked.Token).ConfigureAwait(false);
            }
            catch
            {
                // Preserve the open/cancellation failure if transport cleanup also fails.
                try
                {
                    await inner.CloseAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                }

                throw;
            }
        }

        /// <inheritdoc/>
        /// <remarks>Application cancellation is observed in addition to any operation cancellation token.</remarks>
        public async Task<bool> ReceiveAsync<T>(ReceiveParameters<T> args, CancellationToken cancellationToken = default)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(applicationToken, cancellationToken);
            linked.Token.ThrowIfCancellationRequested();
            return await inner.ReceiveAsync(args, linked.Token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        /// <remarks>Application cancellation is observed in addition to any operation cancellation token.</remarks>
        public async ValueTask SendAsync(ReadOnlyMemory<byte> data, string? receiver = null, CancellationToken cancellationToken = default)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(applicationToken, cancellationToken);
            linked.Token.ThrowIfCancellationRequested();
            await inner.SendAsync(data, receiver, linked.Token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        /// <remarks>Application cancellation is observed in addition to any operation cancellation token.</remarks>
        public void Open() => OpenAsync().GetAwaiter().GetResult();
        /// <inheritdoc/>
        /// <remarks>Application cancellation is observed in addition to any operation cancellation token.</remarks>
        public bool Receive<T>(ReceiveParameters<T> args) => ReceiveAsync(args).GetAwaiter().GetResult();
        /// <inheritdoc/>
        /// <remarks>Application cancellation is observed in addition to any operation cancellation token.</remarks>
        public void Send(object data, string receiver)
        {
            applicationToken.ThrowIfCancellationRequested();
            inner.Send(data, receiver);
        }

        // Closing must remain possible after application cancellation.
        /// <inheritdoc/>
        public Task CloseAsync() => inner.CloseAsync();
        /// <inheritdoc/>
        public void Close() => inner.Close();
        /// <inheritdoc/>
        public IDisposable BeginSynchronous() => inner.BeginSynchronous();
        /// <inheritdoc/>
        public event EventHandler<ReceiveEventArgs>? OnReceived { add => inner.OnReceived += value; remove => inner.OnReceived -= value; }

        /// <inheritdoc/>
        public event EventHandler<MediaErrorEventArgs>? OnError { add => inner.OnError += value; remove => inner.OnError -= value; }

        /// <inheritdoc/>
        public event EventHandler<MediaStateEventArgs>? OnMediaStateChange { add => inner.OnMediaStateChange += value; remove => inner.OnMediaStateChange -= value; }

        /// <inheritdoc/>
        public event EventHandler<TraceEventArgs>? OnTrace { add => inner.OnTrace += value; remove => inner.OnTrace -= value; }

        /// <inheritdoc/>
        public void Copy(object target) => inner.Copy(target);
        /// <inheritdoc/>
        public string Name => inner.Name;
        /// <inheritdoc/>
        public TraceLevel Trace { get => inner.Trace; set => inner.Trace = value; }
        /// <inheritdoc/>
        public bool IsOpen => inner.IsOpen;

        /// <inheritdoc/>
        public bool Properties(IGXWindow? parent = null) => inner.Properties(parent);
        /// <inheritdoc/>
        public Task<bool> PropertiesAsync(IGXWindow? parent = null, CancellationToken cancellationToken = default) => inner.PropertiesAsync(parent, cancellationToken);
        /// <inheritdoc/>
        public IGXWindow? PropertiesForm => inner.PropertiesForm;
        /// <inheritdoc/>
        public string MediaType => inner.MediaType;
        /// <inheritdoc/>
        public bool Enabled => inner.Enabled;
        /// <inheritdoc/>
        public string? Settings { get => inner.Settings; set => inner.Settings = value; }
        /// <inheritdoc/>
        public string? JsonSettings { get => inner.JsonSettings; set => inner.JsonSettings = value; }
        /// <inheritdoc/>
        public object? Synchronous => inner.Synchronous;
        /// <inheritdoc/>
        public bool IsSynchronous => inner.IsSynchronous;

        /// <inheritdoc/>
        public void ResetSynchronousBuffer() => inner.ResetSynchronousBuffer();
        /// <inheritdoc/>
        public ulong BytesSent => inner.BytesSent;
        /// <inheritdoc/>
        public ulong BytesReceived => inner.BytesReceived;

        /// <inheritdoc/>
        public void ResetByteCounters() => inner.ResetByteCounters();
        /// <inheritdoc/>
        public IReadOnlyList<GXValidationError> Validate() => inner.Validate();
        /// <inheritdoc/>
        public object? Eop { get => inner.Eop; set => inner.Eop = value; }
        /// <inheritdoc/>
        public int ConfigurableSettings { get => inner.ConfigurableSettings; set => inner.ConfigurableSettings = value; }
        /// <inheritdoc/>
        public object SyncRoot => inner.SyncRoot;
    }
}
