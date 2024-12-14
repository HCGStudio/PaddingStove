using System.Text.Json.Serialization;

namespace HCGStudio.PaddingStove.Hosting.External;

[JsonConverter(typeof(JsonStringEnumConverter<DeviceType>))]
public enum DeviceType
{
    Unknown,
    [JsonStringEnumMemberName("iPad")] IPad,
    [JsonStringEnumMemberName("iPhone")] IPhone,
    AppleTV
}

public readonly record struct DeviceInfo(string Id, bool CanConnect, string? DeviceName, DeviceType DeviceType);