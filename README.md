# Gurux.Common

Shared media interfaces, event arguments and receive buffering for the Gurux
Device Framework. Targets .NET 9, .NET 10 and .NET 11 without UI framework dependencies.
Concrete transport implementations are provided by separate media libraries.

For request/reply exchanges, call `BeginSynchronous()` before sending, then await
`ReceiveAsync()` and dispose the scope after the exchange. Use `byte[]` or
UTF-8 `string` replies. Receive conditions are documented on `ReceiveParameters<T>`:

| Option | Behavior |
| --- | --- |
| `Count` | Exact byte count without EOP; minimum packet length with EOP. |
| `Eop` | Terminator included in the reply; an object array specifies alternatives. |
| `AllData` | Returns all buffered bytes; timeout may return partial data with `false`. |
| `Peek` | Leaves returned bytes in the buffer. |
| `WaitTime` | Milliseconds including reader queue time; `0` polls and `-1` waits indefinitely. |

Replies accumulate in `Reply`; returning bytes resets `Count` to zero.
Cancellation throws without consuming unread bytes. Closing wakes pending reads
with `false` without flushing partial data; starting a receive on closed media
throws. Reopening clears the buffer and isolates readers from the old connection.

Localized messages are stored in `Development/Properties/Resources.resx`;
typed resource properties are generated during the build.

License: [GPL-2.0-only](LICENSE), or a separate
[Gurux commercial license](LICENSES/LicenseRef-Gurux-Commercial).
See [publishing instructions](docs/publishing.md) and [Gurux](https://www.gurux.fi/).
