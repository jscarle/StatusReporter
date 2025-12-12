using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;

namespace StatusReporter;

/// <summary>Provides the status of the application by implementing the <see cref="IStatusReporter"/> interface.</summary>
internal sealed class StatusReporter : IStatusReporter
{
    private static readonly Assembly EntryAssembly = GetEntryAssembly();

    /// <summary>The name of the entry assembly.</summary>
    private static readonly string AssemblyName = GetAssemblyName();

    /// <summary>The version of the entry assembly.</summary>
    private static readonly string AssemblyVersion = GetAssemblyVersion();

    /// <summary>The last modified date of the entry assembly.</summary>
    private static readonly DateTimeOffset LastModified = GetLastModified();

    /// <summary>The startup time of the application.</summary>
    private static readonly DateTimeOffset Startup = DateTimeOffset.UtcNow;

    private readonly StatusReporterOptions _options;
    private readonly IHostEnvironment? _hostEnvironment;

    /// <summary>Initializes a new instance of the <see cref="StatusReporter"/> class.</summary>
    /// <param name="options">The <see cref="StatusReporterOptions"/> instance that provides configuration options for the status reporter.</param>
    /// <param name="hostEnvironment">The <see cref="IHostEnvironment"/> instance that provides information about the hosting environment.</param>
    public StatusReporter(StatusReporterOptions options, IHostEnvironment? hostEnvironment = null)
    {
        _options = options;
        _hostEnvironment = hostEnvironment;
    }

    /// <inheritdoc/>
    public ApplicationStatus GetStatus()
    {
        var currentTime = DateTimeOffset.UtcNow;

        return new ApplicationStatus
        {
            Assembly = AssemblyName,
            Version = AssemblyVersion,
            BuiltOn = TimeZoneInfo.ConvertTime(LastModified, _options.TimeZone),
            Framework = RuntimeInformation.FrameworkDescription,
            Environment = _hostEnvironment?.EnvironmentName ?? "Unknown",
            OperatingSystem = RuntimeInformation.OSDescription,
            Hostname = Environment.MachineName,
            StartedOn = TimeZoneInfo.ConvertTime(Startup, _options.TimeZone),
            Current = TimeZoneInfo.ConvertTime(currentTime, _options.TimeZone),
            Uptime = currentTime.Subtract(Startup),
        };
    }

    private static Assembly GetEntryAssembly()
    {
        return Assembly.GetEntryAssembly() ?? throw new UnreachableException("Could not get entry assembly.");
    }

    private static string GetAssemblyName()
    {
        return EntryAssembly.GetName().Name ?? throw new UnreachableException("Could not get assembly name.");
    }

    private static string GetAssemblyVersion()
    {
        var informationalVersion = EntryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0];
        var assemblyVersion = EntryAssembly.GetName().Version?.ToString(3) ?? throw new UnreachableException("Could not get assembly version.");
        return informationalVersion ?? assemblyVersion;
    }

    [UnconditionalSuppressMessage("SingleFile", "IL3000:Avoid accessing Assembly file path when publishing as a single file",
        Justification = "Fallbacks have been added to handle cases where the assembly is a single file."
    )]
    private static DateTimeOffset GetLastModified()
    {
        var buildTimestamp = EntryAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(attr => attr.Key == "BuildTimestamp")?.Value;
        if (!string.IsNullOrEmpty(buildTimestamp)
            && DateTimeOffset.TryParse(buildTimestamp, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out var parsedTimestamp))
            return parsedTimestamp;

        if (!string.IsNullOrEmpty(EntryAssembly.Location) && File.Exists(EntryAssembly.Location))
            return new DateTimeOffset(File.GetLastWriteTimeUtc(EntryAssembly.Location), TimeSpan.Zero);

        return DateTimeOffset.MinValue;
    }
}
