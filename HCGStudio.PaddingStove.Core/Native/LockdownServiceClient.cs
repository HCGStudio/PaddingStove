using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace HCGStudio.PaddingStove.Core.Native;

[MustDisposeResource]
internal sealed partial class LockdownServiceClient(nint client) : Stream
{
    protected override void Dispose(bool disposing)
    {
        FreeLockdownServiceClient(client);
        base.Dispose(disposing);
    }

    public override void Flush()
    {
    }

    public override unsafe int Read(byte[] buffer, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset + count > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        fixed (byte* ptr = buffer)
        {
            LibIMobileDevice.ThrowIfError(ServiceReceive(client, ptr + offset, (nuint)count, out var received));

            return (int)received;
        }
    }

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    public override void SetLength(long value) =>
        throw new NotSupportedException();

    public override unsafe void Write(byte[] buffer, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset + count > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        fixed (byte* ptr = buffer)
        {
            LibIMobileDevice.ThrowIfError(ServiceSend(client, ptr + offset, (nuint)count, out var sent));
            
            if (sent < (nuint)count)
            {
                throw new IOException($"Could not send all data, only {sent} bytes sent.");
            }
        }
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "service_client_free")]
    private static unsafe partial External.LockdownServiceErrorStatus FreeLockdownServiceClient(nint client);

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "service_send")]
    private static unsafe partial External.LockdownServiceErrorStatus ServiceSend(
        nint client,
        byte* data,
        nuint size,
        out nuint sent);

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "service_receive")]
    private static unsafe partial External.LockdownServiceErrorStatus ServiceReceive(
        nint client,
        byte* data,
        nuint size,
        out nuint received);

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "service_receive_with_timeout")]
    private static unsafe partial External.LockdownServiceErrorStatus ServiceReceiveWithTimeout(
        nint client,
        byte* data,
        nuint size,
        out nuint received,
        uint timeout);
}