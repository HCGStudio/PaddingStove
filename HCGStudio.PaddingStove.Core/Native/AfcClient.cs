using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace HCGStudio.PaddingStove.Core.Native;

[MustDisposeResource]
internal readonly partial struct AfcClient(nint client) : IDisposable
{
    public void WriteAllBytes(string path, ReadOnlySpan<byte> data)
    {
        LibIMobileDevice.ThrowIfError(FileOpen(client, path, AfcFileMode.WriteOnly, out var handle));
        try
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    var remaining = (uint)data.Length;
                    var cursor = ptr;
                    while (remaining > 0)
                    {
                        LibIMobileDevice.ThrowIfError(FileWrite(client, handle, cursor, remaining, out var written));
                        if (written == 0)
                            throw new IOException($"AFC write to {path} stalled with {remaining} bytes remaining.");
                        cursor += written;
                        remaining -= written;
                    }
                }
            }
        }
        finally
        {
            FileClose(client, handle);
        }
    }

    public void Dispose()
    {
        FreeAfcClient(client);
    }

    private enum AfcFileMode : uint
    {
        ReadOnly = 1,
        ReadWrite = 2,
        WriteOnly = 3,
        WriteRead = 4,
        Append = 5,
        ReadAppend = 6
    }

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "afc_client_free")]
    private static partial External.AfcErrorStatus FreeAfcClient(nint client);

    [LibraryImport(
        "libimobiledevice-1.0",
        EntryPoint = "afc_file_open",
        StringMarshalling = StringMarshalling.Utf8)]
    private static partial External.AfcErrorStatus FileOpen(
        nint client,
        string filename,
        AfcFileMode mode,
        out ulong handle);

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "afc_file_close")]
    private static partial External.AfcErrorStatus FileClose(nint client, ulong handle);

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "afc_file_write")]
    private static unsafe partial External.AfcErrorStatus FileWrite(
        nint client,
        ulong handle,
        byte* data,
        uint length,
        out uint bytesWritten);
}
