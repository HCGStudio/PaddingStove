using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace HCGStudio.PaddingStove.Core.Native;

[MustDisposeResource]
internal readonly partial struct LockdownService(nint device, nint service) : IDisposable
{
    public LockdownServiceClient CreatServiceClient()
    {
        LibIMobileDevice.ThrowIfError(NewLockdownServiceClient(device, service, out var client));
        return new(client);
    }

    public void Dispose()
    {
        FreeLockdownService(service);
    }

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "service_client_new")]
    private static unsafe partial External.LockdownErrorStatus NewLockdownServiceClient(
        nint device,
        nint service,
        out nint client);

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "lockdownd_service_descriptor_free")]
    private static unsafe partial External.LockdownErrorStatus FreeLockdownService(nint service);
}