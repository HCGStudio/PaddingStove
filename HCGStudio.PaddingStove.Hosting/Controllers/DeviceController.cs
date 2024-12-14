using HCGStudio.PaddingStove.Core;
using Microsoft.AspNetCore.Mvc;

namespace HCGStudio.PaddingStove.Hosting.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class DeviceController(IDeviceProvider deviceProvider) : ControllerBase
{
    [HttpGet]
    public IEnumerable<External.DeviceInfo> Get()
    {
        return deviceProvider.GetDevices().Select(ToExternal);
    }

    private static External.DeviceInfo ToExternal(DeviceInfo deviceInfo) => 
        new(deviceInfo.Id, deviceInfo.CanConnect, deviceInfo.DeviceName, (External.DeviceType)deviceInfo.DeviceType);
}