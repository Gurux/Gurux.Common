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

using System.ComponentModel;
using System.Diagnostics;

namespace Gurux.Common
{
    /// <summary>
    /// Defines transport-independent communication operations and events.
    /// </summary>
    /// <remarks>
    /// Events may be raised on transport threads. UI consumers must marshal handlers to their UI thread.
    /// While synchronous receive mode is active, incoming data is buffered for Receive instead of being delivered through OnReceived.
    /// </remarks>
    public interface IGXMedia
    {
        /// <summary>Occurs when received data is delivered.</summary>
        event EventHandler<ReceiveEventArgs>? OnReceived;

        /// <summary>Occurs when a communication error is reported.</summary>
        event EventHandler<MediaErrorEventArgs>? OnError;

        /// <summary>Occurs when the media reports a connection state change.</summary>
        event EventHandler<MediaStateEventArgs>? OnMediaStateChange;

        /// <summary>Occurs when the media emits a trace message.</summary>
        event EventHandler<TraceEventArgs>? OnTrace;

        /// <summary>
        /// Copies the media configuration to another instance.
        /// </summary>
        /// <param name="target">The destination instance supported by the implementation.</param>
        void Copy(object target);

        /// <remarks>
        /// Media name is used to identify media connection,
        /// so two different media connection can not return same media name.
        /// </remarks>
        /// <summary>
        /// Returns name of the media.
        /// </summary>
        string Name
        {
            get;
        }

        /// <seealso cref="OnTrace"/>
        /// <summary>
        /// The trace level specifies which types of trace messages are emitted.
        /// </summary>
        TraceLevel Trace
        {
            get;
            set;
        }

        /// <summary>
        /// Opens the media.
        /// </summary>
        void Open();

        /// <summary>
        /// Checks if the connection is established.
        /// </summary>
        /// <returns>True, if the connection is established.</returns>
        bool IsOpen
        {
            get;
        }

        /// <summary>
        /// Closes the active connection.
        /// </summary>
        /// <seealso cref="Open">Open</seealso>
        void Close();

        /// <summary>
        /// Shows the transport-specific configuration UI on the calling UI thread.
        /// </summary>
        /// <param name="parent">The parent window, or null for no parent.</param>
        /// <returns>True if changes are accepted; false if canceled or no configuration UI is available.</returns>
        bool Properties(IGXWindow? parent = null);

        /// <summary>
        /// Shows the transport-specific configuration UI asynchronously.
        /// </summary>
        /// <param name="parent">The parent window, or null for no parent.</param>
        /// <param name="cancellationToken">A token requesting cancellation of the UI operation.</param>
        /// <returns>True if changes are accepted; false if canceled by the user or no UI is available.</returns>
        /// <exception cref="OperationCanceledException">The operation is canceled using the token.</exception>
        /// <remarks>
        /// Call on the platform UI thread. The media owns and releases its window resources.
        /// Complete the task when the configuration UI closes. Token cancellation must close the UI and cancel the task.
        /// </remarks>
        Task<bool> PropertiesAsync(IGXWindow? parent = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the transport-specific properties window, or null when no configuration UI is available.
        /// Reading this property must not show the window. The media owns its window resources.
        /// </summary>
        IGXWindow? PropertiesForm
        {
            get;
        }

        /// <summary>
        /// Sends data without waiting for a reply from the receiver.
        /// </summary>
        /// <param name="data">Data to send to the device.</param>
        /// <param name="receiver">Media depend information of the receiver (optional).</param>
        /// <seealso cref="Receive">Receive</seealso>
        void Send(object data, string receiver);

        /// <summary>
        /// Returns media type as a string.
        /// </summary>
        string MediaType
        {
            get;
        }

        /// <summary>
        /// Gets whether the media is available for use.
        /// </summary>
        /// <remarks>
        /// Media is disabled example if there is no serial port in the machine.
        /// </remarks>
        bool Enabled
        {
            get;
        }

        /// <summary>
        /// Gets or replaces media settings in the legacy XML format.
        /// </summary>
        string? Settings
        {
            get;
            set;
        }

        /// <summary>Gets or replaces the same media settings as <see cref="Settings"/> using JSON.</summary>
        /// <remarks>Changing either serialized representation updates the transport configuration used by both properties.
        /// Implementations apply the same lifecycle restrictions to XML and JSON settings changes.</remarks>
        string? JsonSettings
        {
            get;
            set;
        }

        /// <summary>
        /// Locking this property makes the connection synchronized and stops sending OnReceived events.
        /// </summary>
        object? Synchronous
        {
            get;
        }

        /// <summary>
        /// Checks if the connection is in synchronous mode.
        /// </summary>
        /// <returns>True, if the connection is in synchronous mode.</returns>
        bool IsSynchronous
        {
            get;
        }

        /// <summary>
        /// Waits synchronously for data according to the supplied receive conditions.
        /// </summary>
        /// <typeparam name="T">The type used to return received data.</typeparam>
        /// <param name="args">Receive conditions and the destination for the received reply.</param>
        /// <returns>True when the receive conditions are satisfied; false on timeout or closure.
        /// With AllData, false on timeout may still append partial bytes to the reply.</returns>
        /// <remarks>
        /// Shared buffering follows <see cref="ReceiveParameters{T}"/>. Returned bytes are consumed unless Peek
        /// is enabled. Closure does not flush partial data. Use asynchronous receive on UI threads.
        /// </remarks>
        /// <seealso cref="Send"/>
        /// <seealso cref="Synchronous">Synchronous</seealso>
        bool Receive<T>(ReceiveParameters<T> args);

        /// <summary>
        /// Resets synchronous buffer.
        /// </summary>
        void ResetSynchronousBuffer();

        /// <summary>
        /// Sent byte count.
        /// </summary>
        /// <seealso cref="BytesReceived">BytesReceived</seealso>
        /// <seealso cref="ResetByteCounters">ResetByteCounters</seealso>
        UInt64 BytesSent
        {
            get;
        }

        /// <summary>
        /// Received byte count.
        /// </summary>
        /// <seealso cref="BytesSent">BytesSent</seealso>
        /// <seealso cref="ResetByteCounters">ResetByteCounters</seealso>
        UInt64 BytesReceived
        {
            get;
        }

        /// <summary>
        /// Resets BytesReceived and BytesSent counters.
        /// </summary>
        /// <seealso cref="BytesSent">BytesSent</seealso>
        /// <seealso cref="BytesReceived">BytesReceived</seealso>
        void ResetByteCounters();

        /// <summary>
        /// Validates the current settings before opening the connection.
        /// </summary>
        void Validate();

        /// <summary>
        /// Data is buffered until EOP is received.
        /// </summary>
        object? Eop
        {
            get;
            set;
        }

        /// <summary>
        /// Visible controls on the properties dialog.
        /// </summary>
        int ConfigurableSettings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an object that contains data to associate with the item.
        /// </summary>
        object? Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an object that can be used to synchronize the media.
        /// </summary>
        [Browsable(false), ReadOnly(true)]
        object SyncRoot
        {
            get;
        }
    }
}
