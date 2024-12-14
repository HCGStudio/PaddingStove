using System.Reflection;
using System.Runtime.InteropServices;

namespace HCGStudio.PaddingStove.Core.Native;

internal static partial class LibIMobileDevice
{
    static LibIMobileDevice()
    {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
    }

    public static unsafe IReadOnlyList<MobileDeviceInfo> GetDeviceList()
    {
        ThrowIfError(GetDeviceList(out var deviceList, out var count));

        var result = new MobileDeviceInfo[count];

        for (var i = 0; i < count; i++)
        {
            result[i] = ToInternal(deviceList[i]);
        }

        // TODO: Release unmanaged memory
        return result;
    }
    
    internal static void ThrowIfError(External.DeviceErrorStatus status)
    {
        if (status != External.DeviceErrorStatus.Success)
            throw new MobileDeviceException<External.DeviceErrorStatus>(status);
    }
    
    internal static void ThrowIfError(External.LockdownErrorStatus status)
    {
        if (status != External.LockdownErrorStatus.Success)
            throw new MobileDeviceException<External.LockdownErrorStatus>(status);
    }
    
    internal static void ThrowIfError(External.LockdownServiceErrorStatus status)
    {
        if (status != External.LockdownServiceErrorStatus.Success)
            throw new MobileDeviceException<External.LockdownServiceErrorStatus>(status);
    }

    [LibraryImport("libimobiledevice-1.0", EntryPoint = "idevice_get_device_list_extended")]
    private static unsafe partial External.DeviceErrorStatus GetDeviceList(
        out External.DeviceInfo** devices,
        out int count);
    
    private static DeviceConnectionType ToInternal(External.DeviceConnectionType type) =>
        (DeviceConnectionType)type;

    private static unsafe MobileDeviceInfo ToInternal(in External.DeviceInfo* device) =>
        new(
            Ensure.NotNull(Marshal.PtrToStringAnsi(device->Udid)),
            ToInternal(device->ConnectionType),
            device->ConnectionData);

    private static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (OperatingSystem.IsMacOS())
        {
            // Use homebrew lib. TODO: Bundle the lib
            return NativeLibrary.Load($"/opt/homebrew/lib/{libraryName}.dylib");
        }

        return NativeLibrary.Load(libraryName, assembly, searchPath);
    }
}

internal readonly record struct MobileDeviceInfo(string Udid, DeviceConnectionType ConnectionType, nint ConnectionData);

internal enum DeviceConnectionType
{
    Usb = 1,
    Network
}