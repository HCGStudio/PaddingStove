using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace HCGStudio.PaddingStove.Core.Native;

[MustDisposeResource]
internal readonly partial struct DeviceConnector : IDisposable
{
    private readonly nint _device;

    public DeviceConnector(string udid)
    {
        LibIMobileDevice.ThrowIfError(NewDevice(
            out _device,
            udid,
            External.DeviceOptions.LookupUsb |
            External.DeviceOptions.LookupNetwork |
            External.DeviceOptions.LookupPreferNetwork));
    }

    public LockDownClient ConnectLockDownClient()
    {
        LibIMobileDevice.ThrowIfError(NewLockdownClient(_device, out var client, nameof(PaddingStove)));
        return new(_device, client);
    }

    public void Dispose()
    {
        FreeDevice(_device);
    }
    
    [LibraryImport(
        "libimobiledevice-1.0",
        EntryPoint = "lockdownd_client_new_with_handshake",
        StringMarshalling = StringMarshalling.Utf8)]
    private static unsafe partial External.LockdownErrorStatus NewLockdownClient(
        nint device,
        out nint client,
        string? label);

    [LibraryImport(
        "libimobiledevice-1.0",
        EntryPoint = "idevice_new_with_options",
        StringMarshalling = StringMarshalling.Utf8)]
    private static unsafe partial External.DeviceErrorStatus NewDevice(
        out nint device,
        string udid,
        External.DeviceOptions options);

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "idevice_free")]
    private static unsafe partial External.DeviceErrorStatus FreeDevice(nint device);
}