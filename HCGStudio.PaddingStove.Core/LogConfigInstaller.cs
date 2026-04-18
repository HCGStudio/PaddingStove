using HCGStudio.PaddingStove.Core.Native;

namespace HCGStudio.PaddingStove.Core;

public interface ILogConfigInstaller
{
    void Install(string deviceId);
}

internal sealed class LogConfigInstaller : ILogConfigInstaller
{
    private const string HearthstoneBundleId = "unity.Blizzard Entertainment.Hearthstone";
    private const string TargetPath = "/log.config";
    private const string ResourceName = "HCGStudio.PaddingStove.Core.log.config";

    private static readonly byte[] LogConfigBytes = LoadLogConfig();

    public void Install(string deviceId)
    {
        using var device = new DeviceConnector(deviceId);
        using var lockdown = device.ConnectLockDownClient();
        using var service = lockdown.StartService("com.apple.mobile.house_arrest");
        using var houseArrest = service.CreateHouseArrestClient();
        houseArrest.SendCommand("VendDocuments", HearthstoneBundleId);
        using var afc = houseArrest.CreateAfcClient();
        afc.WriteAllBytes(TargetPath, LogConfigBytes);
    }

    private static byte[] LoadLogConfig()
    {
        using var stream = typeof(LogConfigInstaller).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
