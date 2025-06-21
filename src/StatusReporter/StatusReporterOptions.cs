namespace StatusReporter;

/// <summary>Represents configuration options for the Status Reporter.</summary>
public sealed class StatusReporterOptions
{
    /// <summary>The time zone used for status date/time reporting. Defaults to UTC.</summary>
    public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;
}
