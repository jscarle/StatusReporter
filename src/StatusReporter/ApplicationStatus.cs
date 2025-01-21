namespace StatusReporter;

/// <summary>Represents the status information of the application.</summary>
public sealed class ApplicationStatus
{
    /// <summary>Gets or sets the assembly name.</summary>
    public required string Assembly { get; init; }

    /// <summary>Gets or sets the version of the assembly.</summary>
    public required string Version { get; set; }

    /// <summary>Gets the built date and time of the assembly.</summary>
    public required string BuiltOn { get; init; }

    /// <summary>Gets the framework of the server.</summary>
    public required string Framework { get; init; }

    /// <summary>Gets the hostname of the server.</summary>
    public required string Hostname { get; init; }

    /// <summary>Gets the operating system of the server.</summary>
    public required string OperatingSystem { get; init; }

    /// <summary>Gets the environment of the server.</summary>
    public required string Environment { get; init; }

    /// <summary>Gets the started date and time on the server.</summary>
    public required string StartedOn { get; init; }

    /// <summary>Gets the current date and time on the server.</summary>
    public required string Current { get; init; }

    /// <summary>Gets the uptime of the server.</summary>
    public required string Uptime { get; init; }
}
