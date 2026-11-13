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

/// <summary>Owned, bounded receive storage with atomic asynchronous search and consumption.</summary>
/// <remarks>A byte array EOP is one pattern; object arrays are alternative patterns.
/// Count is the minimum packet length when EOP is specified. WaitTime zero polls;
/// -1 waits indefinitely. Close wakes all readers. Returned payloads own their storage.</remarks>
public sealed class GXSynchronousMediaBase
{
    /// <summary>Serializes access to the receive storage and connection generation.</summary>
    private readonly object sync = new();
    /// <summary>Allows one receive operation to inspect and consume bytes at a time.</summary>
    private readonly SemaphoreSlim reader = new(1, 1);
    /// <summary>Maximum number of unread bytes permitted in owned storage.</summary>
    private readonly int maximumSize;
    /// <summary>Owned byte storage containing the unread region.</summary>
    private byte[] buffer;
    /// <summary>Offset of the first unread byte in owned storage.</summary>
    private int start;
    /// <summary>Number of unread bytes in owned storage.</summary>
    private int length;
    /// <summary>Connection generation used to isolate pending readers after lifecycle changes.</summary>
    private int generation;
    /// <summary>Whether the current connection generation is closed.</summary>
    private bool closed;
    /// <summary>Transport failure rethrown by pending receive operations.</summary>
    private Exception? error;
    /// <summary>Signal replaced whenever receive state changes.</summary>
    private TaskCompletionSource changed = NewSignal();
    /// <summary>Creates a buffer with a configurable upper bound (16 MiB by default).</summary>
    /// <param name="bufferSize">Initial storage capacity in bytes.</param>
    /// <param name="maximumSize">Maximum unread storage capacity in bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException">The initial size is nonpositive or exceeds the maximum.</exception>
    public GXSynchronousMediaBase(int bufferSize = 4096, int maximumSize = 16 * 1024 * 1024)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
        if (maximumSize < bufferSize)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumSize));
        }

        buffer = new byte[bufferSize];
        this.maximumSize = maximumSize;
    }

    /// <summary>Number of bytes currently buffered.</summary>
    public int Count
    {
        get
        {
            lock (sync)
            {
                return length;
            }
        }
    }

    /// <summary>Creates a signal whose continuations run outside the buffer lock.</summary>
    /// <returns>An incomplete asynchronous change signal.</returns>
    private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    /// <summary>Replaces and completes the current change signal to wake pending readers.</summary>
    private void Pulse()
    {
        var previous = changed;
        changed = NewSignal();
        previous.TrySetResult();
    }

    /// <summary>Starts a new connection generation, clearing buffered bytes and errors.</summary>
    public void Open()
    {
        lock (sync)
        {
            generation++;
            closed = false;
            error = null;
            start = length = 0;
            Pulse();
        }
    }

    /// <summary>Closes the current generation and wakes pending readers.</summary>
    public void Close()
    {
        lock (sync)
        {
            closed = true;
            generation++;
            Pulse();
        }
    }

    /// <summary>Clears unread bytes and resets incremental searches.</summary>
    public void Reset()
    {
        lock (sync)
        {
            start = length = 0;
            generation++;
            Pulse();
        }
    }

    /// <summary>Records a transport failure and wakes readers.</summary>
    /// <param name="exception">Transport failure to rethrow from pending receives.</param>
    /// <exception cref="ArgumentNullException">The exception is null.</exception>
    public void SetException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        lock (sync)
        {
            error = exception;
            Pulse();
        }
    }

    /// <summary>Copies a source range into owned storage.</summary>
    /// <param name="data">Source array containing the bytes to copy.</param>
    /// <param name="index">First source byte to copy.</param>
    /// <param name="count">Number of bytes to copy.</param>
    /// <exception cref="ArgumentOutOfRangeException">The source range is invalid.</exception>
    public void AppendData(byte[] data, int index, int count) => AppendData(data.AsSpan(index, count));
    /// <summary>Copies data into owned storage and wakes pending readers.</summary>
    /// <remarks>Empty input and input received while closed are ignored.</remarks>
    /// <param name="data">Bytes copied into owned unread storage.</param>
    /// <exception cref="InvalidOperationException">The unread data would exceed the configured capacity.</exception>
    public void AppendData(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return;
        }

        lock (sync)
        {
            if (closed)
            {
                return;
            }

            if (data.Length > maximumSize - length)
            {
                throw new InvalidOperationException(Properties.Resources.ReceiveBufferCapacityExceeded);
            }

            int required = length + data.Length;
            if (required > buffer.Length)
            {
                var next = new byte[(int)Math.Min(maximumSize, Math.Max((long)required, (long)buffer.Length * 2))];
                buffer.AsSpan(start, length).CopyTo(next);
                buffer = next;
                start = 0;
            }
            else if (start + required > buffer.Length)
            {
                buffer.AsSpan(start, length).CopyTo(buffer);
                start = 0;
            }

            data.CopyTo(buffer.AsSpan(start + length));
            length = required;
            Pulse();
        }
    }

    /// <summary>Waits synchronously. Use ReceiveAsync on UI/browser threads.</summary>
    /// <typeparam name="T">Byte array or UTF-8 string reply type.</typeparam>
    /// <param name="args">Receive conditions and accumulated reply.</param>
    /// <returns>Whether the requested receive conditions were satisfied.</returns>
    public bool Receive<T>(ReceiveParameters<T> args) => ReceiveAsync(args).GetAwaiter().GetResult();
    /// <summary>Waits for a packet without blocking a thread.</summary>
    /// <remarks>
    /// Matching and reply accumulation follow <see cref="ReceiveParameters{T}"/>. Only one reader can inspect
    /// or consume storage at a time; WaitTime includes time waiting for that reader slot.
    /// Cancellation preserves unread bytes. Timeout preserves them unless AllData returns a partial reply
    /// after the reader slot is acquired. Returned bytes are consumed unless Peek is enabled.
    /// Close, Open or Reset invalidates pending readers, which return false without returning partial data.
    /// Open clears stored bytes and transport errors. A recorded transport failure is rethrown to the reader.
    /// </remarks>
    /// <typeparam name="T">Byte array or UTF-8 string reply type.</typeparam>
    /// <param name="args">Receive conditions and accumulated reply.</param>
    /// <param name="cancellationToken">Token canceling the wait without consuming unread bytes.</param>
    /// <returns>True if packet conditions are satisfied; false on timeout or an invalidated connection generation.
    /// False on timeout may still append partial bytes to Reply when AllData is enabled.</returns>
    /// <exception cref="ArgumentNullException">The receive parameters are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Count is negative or the wait time is less than -1.</exception>
    /// <exception cref="ArgumentException">No receive condition is set or a terminator is empty.</exception>
    /// <exception cref="NotSupportedException">The reply type is unsupported.</exception>
    /// <exception cref="OperationCanceledException">The receive is canceled.</exception>
    public async Task<bool> ReceiveAsync<T>(ReceiveParameters<T> args, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Count < 0 || args.WaitTime < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(args));
        }

        byte[][] patterns = Patterns(args.Eop);
        if (patterns.Length == 0 && args.Count == 0 && !args.AllData)
        {
            throw new ArgumentException(Properties.Resources.ReceiveConditionsRequired, nameof(args));
        }

        long began = Stopwatch.GetTimestamp();
        int connection;
        lock (sync)
        {
            connection = generation;
        }

        if (!await reader.WaitAsync(Remaining(args.WaitTime, began), cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        try
        {
            int[] scanned = new int[patterns.Length];
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Task signal;
                lock (sync)
                {
                    if (connection != generation || closed)
                    {
                        return false;
                    }

                    if (error != null)
                    {
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
                    }

                    int packet = -1;
                    ReadOnlySpan<byte> data = buffer.AsSpan(start, length);
                    if (patterns.Length == 0)
                    {
                        int needed = Math.Max(1, args.Count);
                        if (length >= needed)
                        {
                            packet = args.AllData ? length : needed;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < patterns.Length; ++i)
                        {
                            var pattern = patterns[i];
                            // A match ending before Count cannot finish this packet.
                            int from = Math.Max(scanned[i], Math.Max(0, args.Count - pattern.Length));
                            if (from <= length)
                            {
                                int match = data[from..].IndexOf(pattern);
                                if (match >= 0)
                                {
                                    int end = from + match + pattern.Length;
                                    if (packet < 0 || end < packet)
                                    {
                                        packet = end;
                                    }
                                }
                            }

                            scanned[i] = Math.Max(0, length - pattern.Length + 1);
                        }
                    }

                    bool satisfied = packet >= 0;
                    bool timedOut = Remaining(args.WaitTime, began) == 0;
                    if (satisfied || (timedOut && args.AllData && length != 0))
                    {
                        if (args.AllData)
                        {
                            packet = length;
                        }

                        byte[] owned = data[..packet].ToArray();
                        // Conversion is validated before consuming bytes.
                        T converted = ConvertReply<T>(owned, args.Reply);
                        if (!args.Peek)
                        {
                            start += packet;
                            length -= packet;
                            if (length == 0)
                            {
                                start = 0;
                            }
                        }

                        args.Reply = converted;
                        args.Count = 0;
                        return satisfied;
                    }

                    if (timedOut)
                    {
                        return false;
                    }

                    signal = changed.Task;
                }

                try
                {
                    if (args.WaitTime < 0)
                    {
                        await signal.WaitAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await signal.WaitAsync(TimeSpan.FromMilliseconds(Remaining(args.WaitTime, began)), cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TimeoutException)
                { /* Recheck under lock before deciding timeout. */
                }
            }
        }
        finally
        {
            reader.Release();
        }
    }

    /// <summary>Calculates remaining timeout using elapsed monotonic time.</summary>
    /// <param name="waitTime">Configured timeout in milliseconds, or -1 for an infinite wait.</param>
    /// <param name="began">Monotonic timestamp at the start of the receive.</param>
    /// <returns>Remaining milliseconds, zero if expired, or -1 for an infinite wait.</returns>
    private static int Remaining(int waitTime, long began) => waitTime < 0
        ? -1
        : Math.Max(0, waitTime - (int)Math.Min(int.MaxValue, Stopwatch.GetElapsedTime(began).TotalMilliseconds));

    /// <summary>Converts one terminator or alternative terminators into nonempty byte patterns.</summary>
    /// <param name="eop">One terminator, alternative terminators, or null for no terminator.</param>
    /// <returns>Owned nonempty byte patterns, or an empty array.</returns>
    /// <exception cref="ArgumentException">A terminator is empty or has an unsupported type.</exception>
    private static byte[][] Patterns(object? eop)
    {
        if (eop == null)
        {
            return[];
        }

        object[] values = eop is object[] alternatives ? alternatives : [eop];
        return values.Select(value =>
        {
            byte[] bytes = GXMediaBase.GetBytes(value);
            if (bytes.Length == 0)
            {
                throw new ArgumentException(Properties.Resources.EmptyEopPattern, nameof(eop));
            }

            return bytes;
        }).ToArray();
    }

    /// <summary>Appends received bytes to a supported accumulated reply type.</summary>
    /// <typeparam name="T">Byte array or UTF-8 string reply type.</typeparam>
    /// <param name="bytes">New packet bytes to append.</param>
    /// <param name="previous">Previously accumulated reply, if any.</param>
    /// <returns>The accumulated reply containing the new packet.</returns>
    /// <exception cref="NotSupportedException">The reply type is unsupported.</exception>
    private static T ConvertReply<T>(byte[] bytes, T? previous)
    {
        object result;
        if (typeof(T) == typeof(byte[]))
        {
            result = previous is byte[] old ? old.Concat(bytes).ToArray() : bytes;
        }
        else if (typeof(T) == typeof(string))
        {
            result = (previous as string ?? string.Empty) + Encoding.UTF8.GetString(bytes);
        }
        else
        {
            throw new NotSupportedException(Properties.Resources.UnsupportedReplyType);
        }

        return (T)result;
    }
}
