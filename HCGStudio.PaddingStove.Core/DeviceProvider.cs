using System.Text;
using HCGStudio.PaddingStove.Core.Native;

namespace HCGStudio.PaddingStove.Core;

internal class DeviceProvider : IDeviceProvider
{
    public IReadOnlyList<DeviceInfo> GetDevices()
    {
        var result = LibIMobileDevice.GetDeviceList();
        return result.Select(ProcessDevice).ToArray();
    }

    private static DeviceInfo ProcessDevice(MobileDeviceInfo device)
    {
        try
        {
            using var connection = new DeviceConnector(device.Udid);
            var client = connection.ConnectLockDownClient();
            var deviceClass = client.GetStringValue(null, "DeviceClass");
            return new(
                device.Udid,
                true,
                client.GetStringValue(null, "DeviceName"),
                Enum.TryParse<DeviceType>(deviceClass, true, out var type) ? type : DeviceType.Unknown);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new(device.Udid, false, null, DeviceType.Unknown);
        }
    }
}