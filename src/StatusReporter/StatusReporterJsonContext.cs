using System.Text.Json.Serialization;

namespace StatusReporter;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(GetStatusResponse))]
internal sealed partial class StatusReporterJsonContext : JsonSerializerContext;
