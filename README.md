# StatusReporter

A status reporter for .NET Web Applications and APIs.

Supports .NET 8, .NET 9, .NET 10, and .NET 11.

## Quick Start

### Register the Status Reporter
```csharp
builder.Services.AddStatusReporter();
```

### Map the Status Endpoint

_(Defaults to /status)_

```csharp
app.MapStatus();
```

### Sample Output
```json
{
  "assembly": "Api",
  "version": "17.3.11-rc.41283",
  "builtOn": "2025-01-21T12:35:24.0000000-05:00",
  "framework": ".NET 9.0.0",
  "hostname": "3f74cbf2ee2",
  "operatingSystem": "Debian GNU/Linux 12 (bookworm)",
  "environment": "Development",
  "startedOn": "2025-01-21T12:40:40.7332471-05:00",
  "current": "2025-01-21T16:00:49.5350510-05:00",
  "uptime": "3:20:08"
}
```

## Trimming and Native AOT

StatusReporter supports trimming and Native AOT on .NET 8, 9, 10, and 11. The library declares `IsAotCompatible`, maps its endpoint with an explicit request delegate, and uses source-generated JSON serialization.

Enable Native AOT publishing in your application with `<PublishAot>true</PublishAot>`. `MapStatus` retains its `RouteHandlerBuilder` return type, so endpoint conventions such as authorization can still be chained.

For a meaningful `BuiltOn` value in native or single-file deployments, provide a parseable `AssemblyMetadataAttribute` named `BuildTimestamp` on your entry assembly. Without it, `BuiltOn` falls back to `DateTimeOffset.MinValue` when the assembly file is unavailable.
