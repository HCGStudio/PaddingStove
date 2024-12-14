using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace HCGStudio.PaddingStove.Core.Native;

[MustDisposeResource]
internal readonly partial struct LockDownClient(nint device, nint client) : IDisposable
{
    public PlistNode GetValue(string? domain, string key)
    {
        LibIMobileDevice.ThrowIfError(LockdownGetValue(client, domain, key, out var node));
        return new(node);
    }

    public string GetStringValue(string? domain, string key)
    {
        LibIMobileDevice.ThrowIfError(LockdownGetValue(client, domain, key, out var node));
        using var plistNode = new PlistNode(node);
        return plistNode.StringValue;
    }

    public LockdownService StartService(string identifier)
    {
        LibIMobileDevice.ThrowIfError(LockdownStartService(client, identifier, out var service));
        return new(device, service);
    }

    public void Dispose()
    {
        FreeLockdownClient(client);
    }


    [LibraryImport("libimobiledevice-1.0", EntryPoint = "lockdownd_client_free")]
    private static unsafe partial External.LockdownErrorStatus FreeLockdownClient(nint client);

    [LibraryImport(
        "libimobiledevice-1.0",
        EntryPoint = "lockdownd_get_value",
        StringMarshalling = StringMarshalling.Utf8)]
    private static unsafe partial External.LockdownErrorStatus LockdownGetValue(
        nint client,
        string? domain,
        string key,
        out nint node);

    [LibraryImport("libimobiledevice-1.0",
        EntryPoint = "lockdownd_start_service",
        StringMarshalling = StringMarshalling.Utf8)]
    private static unsafe partial External.LockdownErrorStatus LockdownStartService(
        nint client,
        string identifier,
        out nint service);
}