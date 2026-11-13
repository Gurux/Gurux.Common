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
using System.Text;

namespace Gurux.Common;

/// <summary>Shared events, receive storage and optional UI plumbing for modern media.</summary>
public abstract class GXMediaBase : IGXAsyncMedia, IDisposable, IAsyncDisposable
{
    /// <summary>Monitor used by legacy synchronous request and reply exchanges.</summary>
    private readonly object synchronous = new();
    /// <summary>Serializes event packet framing and end-of-packet changes.</summary>
    private readonly object framing = new();
    /// <summary>Accumulates unsolicited input until complete event packets are available.</summary>
    private readonly GXSynchronousMediaBase eventBuffer = new();
    /// <summary>Current event packet terminator or alternative terminators.</summary>
    private object? eop;
    /// <summary>Number of active asynchronous buffered receive scopes.</summary>
    private int receiveScopes;
    /// <summary>Atomic counter for successfully transmitted bytes.</summary>
    private long bytesSent;
    /// <summary>Atomic counter for received bytes.</summary>
    private long bytesReceived;
    /// <summary>Owned receive buffer for this connection.</summary>
    protected GXSynchronousMediaBase ReceiveBuffer { get; } = new();

    /// <inheritdoc/>
    public event EventHandler<ReceiveEventArgs>? OnReceived;
    /// <inheritdoc/>
    public event EventHandler<MediaErrorEventArgs>? OnError;
    /// <inheritdoc/>
    public event EventHandler<MediaStateEventArgs>? OnMediaStateChange;
    /// <inheritdoc/>
    public event EventHandler<TraceEventArgs>? OnTrace;
    /// <summary>Optional settings UI adapter supplied by the application.</summary>
    public IGXMediaSettingsEditor? SettingsEditor { get; set; }
    /// <inheritdoc/>
    public IGXWindow? PropertiesForm => SettingsEditor?.Window;
    /// <inheritdoc/>
    public virtual string Name => GetType().Name;
    /// <inheritdoc/>
    public virtual string MediaType => GetType().Name;
    /// <inheritdoc/>
    public TraceLevel Trace { get; set; }
    /// <inheritdoc/>
    public abstract bool IsOpen { get; }
    /// <inheritdoc/>
    public virtual bool Enabled => true;
    /// <inheritdoc/>
    public virtual string? Settings { get; set; }

    /// <inheritdoc/>
    public object? Eop
    {
        get
        {
            lock (framing)
            {
                return eop;
            }
        }

        set
        {
            lock (framing)
            {
                eop = value;
                eventBuffer.Reset();
            }
        }
    }

    /// <inheritdoc/>
    public int ConfigurableSettings { get; set; } = -1;
    /// <inheritdoc/>
    public object? Tag { get; set; }
    /// <inheritdoc/>
    public object SyncRoot { get; } = new();
    /// <inheritdoc/>
    public object Synchronous => synchronous;

    /// <inheritdoc/>
    public bool IsSynchronous
    {
        get
        {
            if (Volatile.Read(ref receiveScopes) != 0 || Monitor.IsEntered(synchronous))
            {
                return true;
            }

            if (!Monitor.TryEnter(synchronous))
            {
                return true;
            }

            Monitor.Exit(synchronous);
            return false;
        }
    }

    /// <summary>Enables buffered receive across awaits. Dispose the scope after the exchange.</summary>
    /// <returns>A scope that restores event delivery when the last active scope is disposed.</returns>
    public IDisposable BeginSynchronous()
    {
        Interlocked.Increment(ref receiveScopes);
        return new ReceiveScope(this);
    }

    /// <summary>Releases one buffered receive scope exactly once.</summary>
    /// <param name="owner">Media whose active receive scope count is decremented on disposal.</param>
    private sealed class ReceiveScope(GXMediaBase owner) : IDisposable
    {
        /// <summary>Owning media until the scope is disposed.</summary>
        private GXMediaBase? media = owner;
        /// <inheritdoc/>
        public void Dispose()
        {
            var value = Interlocked.Exchange(ref media, null);
            if (value != null)
            {
                Interlocked.Decrement(ref value.receiveScopes);
            }
        }
    }

    /// <inheritdoc/>
    public ulong BytesSent => (ulong)Interlocked.Read(ref bytesSent);
    /// <inheritdoc/>
    public ulong BytesReceived => (ulong)Interlocked.Read(ref bytesReceived);

    /// <inheritdoc/>
    public void ResetByteCounters()
    {
        Interlocked.Exchange(ref bytesSent, 0);
        Interlocked.Exchange(ref bytesReceived, 0);
    }

    /// <summary>Accounts for successfully sent bytes.</summary>
    /// <param name="count">Number of successfully transmitted bytes.</param>
    protected void AddBytesSent(int count) => Interlocked.Add(ref bytesSent, count);
    /// <summary>Accounts for received bytes routed to an attached session.</summary>
    /// <param name="count">Number of received session bytes.</param>
    protected void AddBytesReceived(int count) => Interlocked.Add(ref bytesReceived, count);
    /// <summary>Delivers bytes either to buffered receives or event subscribers.</summary>
    /// <param name="data">Newly received bytes.</param>
    /// <param name="sender">Transport-specific sender identification.</param>
    /// <param name="packetBuffer">Optional session-specific event framing buffer.</param>
    protected void PublishReceived(byte[] data, string? sender = null, GXSynchronousMediaBase? packetBuffer = null)
    {
        Interlocked.Add(ref bytesReceived, data.Length);
        if (Trace == TraceLevel.Verbose)
        {
            OnTrace?.Invoke(this, new TraceEventArgs(TraceTypes.Received, data.ToArray(), sender));
        }

        if (IsSynchronous)
        {
            ReceiveBuffer.AppendData(data);
        }
        else
        {
            List<byte[]> packets = [];
            lock (framing)
            {
                if (eop == null)
                {
                    packets.Add(data);
                }
                else
                {
                    var pending = packetBuffer ?? eventBuffer;
                    pending.AppendData(data);
                    while (true)
                    {
                        var packet = new ReceiveParameters<byte[]>
                        {
                            Eop = eop,
                            WaitTime = 0
                        };
                        if (!pending.Receive(packet))
                        {
                            break;
                        }

                        packets.Add(packet.Reply!);
                    }
                }
            }

            foreach (var packet in packets)
            {
                OnReceived?.Invoke(this, new ReceiveEventArgs(packet, sender));
            }
        }
    }

    /// <summary>Reports a transport error and wakes pending reads.</summary>
    /// <param name="error">Transport failure to expose to readers and subscribers.</param>
    protected void ReportError(Exception error)
    {
        ReceiveBuffer.SetException(error);
        if (Trace >= TraceLevel.Error)
        {
            OnTrace?.Invoke(this, new TraceEventArgs(TraceTypes.Error, error, null));
        }

        OnError?.Invoke(this, new MediaErrorEventArgs(error));
    }

    /// <summary>Raises a connection lifecycle event.</summary>
    /// <param name="state">Connection state to publish and apply to the event buffer.</param>
    protected void SetState(MediaState state)
    {
        lock (framing)
        {
            if (state == MediaState.Open)
            {
                eventBuffer.Open();
            }
            else if (state == MediaState.Closed)
            {
                eventBuffer.Close();
            }
        }

        OnMediaStateChange?.Invoke(this, new MediaStateEventArgs(state));
    }

    /// <inheritdoc/>
    public abstract Task OpenAsync(CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    public abstract Task CloseAsync();
    /// <summary>Sends bytes without blocking the calling thread.</summary>
    /// <inheritdoc cref="IGXAsyncMedia.SendAsync"/>
    public abstract ValueTask SendAsync(ReadOnlyMemory<byte> data, string? receiver = null, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    public void Open() => OpenAsync().GetAwaiter().GetResult();
    /// <inheritdoc/>
    public void Close() => CloseAsync().GetAwaiter().GetResult();
    /// <inheritdoc/>
    public void Send(object data, string receiver) => SendAsync(GetBytes(data), receiver).AsTask().GetAwaiter().GetResult();
    /// <summary>Sends to the default destination.</summary>
    /// <param name="data">Supported byte or text payload to transmit.</param>
    public void Send(object data) => SendAsync(GetBytes(data)).AsTask().GetAwaiter().GetResult();
    /// <summary>Converts supported media payloads into owned bytes (UTF-8 for text).</summary>
    /// <param name="data">Byte array, byte, character, text or byte memory to copy.</param>
    /// <returns>An independent byte array containing the payload.</returns>
    /// <exception cref="ArgumentException">The payload type is unsupported.</exception>
    public static byte[] GetBytes(object data) => data switch
    {
        byte[] bytes => bytes.ToArray(),
        string text => Encoding.UTF8.GetBytes(text),
        byte value => [value],
        char value => Encoding.UTF8.GetBytes(value.ToString()),
        ReadOnlyMemory<byte> memory => memory.ToArray(),
        Memory<byte> memory => memory.ToArray(),
        _ => throw new ArgumentException(global::Gurux.Common.Properties.Resources.ExpectedBytesOrText, nameof(data))
    };
    /// <inheritdoc/>
    public virtual bool Receive<T>(ReceiveParameters<T> args) => ReceiveAsync(args).GetAwaiter().GetResult();
    /// <summary>Receives from the buffer. Use BeginSynchronous before sending the request.</summary>
    /// <inheritdoc cref="IGXAsyncMedia.ReceiveAsync{T}"/>
    /// <exception cref="InvalidOperationException">The media is closed.</exception>
    public virtual Task<bool> ReceiveAsync<T>(ReceiveParameters<T> args, CancellationToken cancellationToken = default)
    {
        if (!IsOpen)
        {
            throw new InvalidOperationException(global::Gurux.Common.Properties.Resources.MediaClosed);
        }

        return ReceiveBuffer.ReceiveAsync(args, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual void ResetSynchronousBuffer() => ReceiveBuffer.Reset();
    /// <inheritdoc/>
    public virtual void Validate()
    {
    }

    /// <inheritdoc/>
    public virtual void Copy(object target)
    {
        if (target is not GXMediaBase media || media.GetType() != GetType())
        {
            throw new ArgumentException(global::Gurux.Common.Properties.Resources.MediaTypesMustMatch, nameof(target));
        }

        media.Settings = Settings;
        media.Trace = Trace;
        media.ConfigurableSettings = ConfigurableSettings;
    }

    /// <inheritdoc/>
    public bool Properties(IGXWindow? parent = null)
    {
        if (SettingsEditor != null)
        {
            throw new NotSupportedException(global::Gurux.Common.Properties.Resources.AsyncSettingsEditorRequired);
        }

        return false;
    }

    /// <inheritdoc/>
    public Task<bool> PropertiesAsync(IGXWindow? parent = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return SettingsEditor?.ShowAsync(this, parent, cancellationToken) ?? Task.FromResult(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
