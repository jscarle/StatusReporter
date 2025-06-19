using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace StatusReporter;

/// <summary>Represents the status information of the application.</summary>
public sealed class ApplicationStatus
{
    /// <summary>Gets or sets the assembly name.</summary>
    public required string Assembly { get; init; }

    /// <summary>Gets or sets the version of the assembly.</summary>
    public required string Version { get; init; }

    /// <summary>Gets the built date and time of the assembly.</summary>
    public required DateTimeOffset BuiltOn { get; init; }

    /// <summary>Gets the framework of the server.</summary>
    public required string Framework { get; init; }

    /// <summary>Gets the hostname of the server.</summary>
    public required string Hostname { get; init; }

    /// <summary>Gets the operating system of the server.</summary>
    public required string OperatingSystem { get; init; }

    /// <summary>Gets the environment of the server.</summary>
    public required string Environment { get; init; }

    /// <summary>Gets the started date and time on the server.</summary>
    public required DateTimeOffset StartedOn { get; init; }

    /// <summary>Gets the current date and time on the server.</summary>
    public required DateTimeOffset Current { get; init; }

    /// <summary>Gets the uptime of the server.</summary>
    public required TimeSpan Uptime { get; init; }

    internal ApplicationStatusJson ToJson()
    {
        return new ApplicationStatusJson
        {
            Assembly = Assembly,
            Version = Version,
            BuiltOn = BuiltOn.ToLocalTime().ToString("O"),
            Framework = Framework,
            Hostname = Hostname,
            OperatingSystem = OperatingSystem,
            Environment = Environment,
            StartedOn = StartedOn.ToLocalTime().ToString("O"),
            Current = Current.ToLocalTime().ToString("O"),
            Uptime = Uptime.ToString("g", CultureInfo.InvariantCulture).Split('.')[0],
        };
    }
}

internal sealed class ApplicationStatusJson
{
    public string Assembly { get; set; } = "";
    public string Version { get; set; } = "";
    public string BuiltOn { get; set; } = "";
    public string Framework { get; set; } = "";
    public string Hostname { get; set; } = "";
    public string OperatingSystem { get; set; } = "";
    public string Environment { get; set; } = "";
    public string StartedOn { get; set; } = "";
    public string Current { get; set; } = "";
    public string Uptime { get; set; } = "";
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(ApplicationStatusJson))]
internal sealed partial class StatusReporterJsonContext : JsonSerializerContext;
