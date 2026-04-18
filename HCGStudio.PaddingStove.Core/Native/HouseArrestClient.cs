using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace HCGStudio.PaddingStove.Core.Native;

[MustDisposeResource]
internal readonly partial struct HouseArrestClient(nint client) : IDisposable
{
    public void SendCommand(string command, string appId)
    {
        LibIMobileDevice.ThrowIfError(SendCommand(client, command, appId));
        LibIMobileDevice.ThrowIfError(GetResult(client, out var result));
        using var resultPlist = new PlistNode(result);
        var error = resultPlist.GetDictStringItem("Error");
        if (error is not null)
            throw new HouseArrestRequestException(command, appId, error);
    }

    public AfcClient CreateAfcClient()
    {
        LibIMobileDevice.ThrowIfError(NewAfcFromHouseArrest(client, out var afc));
        return new(afc);
    }

    public void Dispose()
    {
        FreeHouseArrestClient(client);
    }

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "house_arrest_client_free")]
    private static partial External.HouseArrestErrorStatus FreeHouseArrestClient(nint client);

    [LibraryImport(
        "libimobiledevice-1.0",
        EntryPoint = "house_arrest_send_command",
        StringMarshalling = StringMarshalling.Utf8)]
    private static partial External.HouseArrestErrorStatus SendCommand(
        nint client,
        string command,
        string appId);

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "house_arrest_get_result")]
    private static partial External.HouseArrestErrorStatus GetResult(nint client, out nint dict);

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "afc_client_new_from_house_arrest_client")]
    private static partial External.AfcErrorStatus NewAfcFromHouseArrest(nint client, out nint afc);
}

public class HouseArrestRequestException(string command, string appId, string error)
    : Exception($"house_arrest {command} for {appId} failed: {error}");
