using HCGStudio.PaddingStove.Core;
using HCGStudio.PaddingStove.Core.Native;
using Microsoft.AspNetCore.Mvc;

namespace HCGStudio.PaddingStove.Hosting.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class DeviceController(
    IDeviceProvider deviceProvider,
    ILogConfigInstaller logConfigInstaller,
    ILogger<DeviceController> logger) : ControllerBase
{
    [HttpGet]
    public IEnumerable<External.DeviceInfo> Get()
    {
        return deviceProvider.GetDevices().Select(ToExternal);
    }

    [HttpPost("{id}/install-log-config")]
    public ActionResult<External.InstallLogConfigResult> InstallLogConfig(string id)
    {
        try
        {
            logConfigInstaller.Install(id);
            return new External.InstallLogConfigResult(true, null);
        }
        catch (HouseArrestRequestException ex)
        {
            logger.LogWarning(ex, "house_arrest rejected log.config install for device {DeviceId}", id);
            return new External.InstallLogConfigResult(false, ex.Message);
        }
        catch (Exception ex) when (ex.GetType().IsGenericType
                                   && ex.GetType().GetGenericTypeDefinition() == typeof(MobileDeviceException<>))
        {
            logger.LogWarning(ex, "Native error installing log.config for device {DeviceId}", id);
            return new External.InstallLogConfigResult(false, ex.Message);
        }
    }

    private static External.DeviceInfo ToExternal(DeviceInfo deviceInfo) =>
        new(deviceInfo.Id, deviceInfo.CanConnect, deviceInfo.DeviceName, (External.DeviceType)deviceInfo.DeviceType);
}
