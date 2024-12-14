using System.Diagnostics.CodeAnalysis;

namespace HCGStudio.PaddingStove.Core;

public interface IDeviceProvider
{
    public IReadOnlyList<DeviceInfo> GetDevices();
}

public enum DeviceType
{
    Unknown,
    IPad,
    IPhone,
    AppleTV
}

public readonly record struct DeviceInfo(
    string Id,
    [property: MemberNotNullWhen(true, "DeviceName")] bool CanConnect,
    string? DeviceName,
    DeviceType DeviceType);