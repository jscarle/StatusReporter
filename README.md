# StatusReporter

A status reporter for .NET Web Applications and APIs.

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
