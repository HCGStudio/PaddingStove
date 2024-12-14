using HCGStudio.PaddingStove.Core.Native;
using Microsoft.Extensions.DependencyInjection;

namespace HCGStudio.PaddingStove.Core;

public static class PaddingStoveCoreExtension
{
    public static IServiceCollection AddPaddingStoveCore(this IServiceCollection services)
    {
        // Warm Up
        LibIMobileDevice.GetDeviceList();
        
        services.AddSingleton<IDeviceProvider, DeviceProvider>();
        services.AddSingleton<IGameBoardFactory, GameBoardFactory>();
        return services;
    }
}