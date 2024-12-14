using System.Text.Json;
using System.Text.Json.Serialization;

namespace HCGStudio.PaddingStove.Hosting;

[JsonSerializable(typeof(External.DeviceInfo))]
[JsonSerializable(typeof(External.ISseEvent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, UseStringEnumConverter = true)]
internal partial class JsonContext : JsonSerializerContext;