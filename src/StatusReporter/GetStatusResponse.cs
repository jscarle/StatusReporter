using System.Globalization;

namespace StatusReporter;

internal sealed class GetStatusResponse
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

internal static class GetStatusResponseExtensions
{
    public static GetStatusResponse ToResponse(this ApplicationStatus applicationStatus)
    {
        return new GetStatusResponse
        {
            Assembly = applicationStatus.Assembly,
            Version = applicationStatus.Version,
            BuiltOn = applicationStatus.BuiltOn.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture),
            Framework = applicationStatus.Framework,
            Hostname = applicationStatus.Hostname,
            OperatingSystem = applicationStatus.OperatingSystem,
            Environment = applicationStatus.Environment,
            StartedOn = applicationStatus.StartedOn.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture),
            Current = applicationStatus.Current.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture),
            Uptime = applicationStatus.Uptime.Days > 0
                ? applicationStatus.Uptime.ToString(@"d\.hh\:mm\:ss", CultureInfo.InvariantCulture)
                : applicationStatus.Uptime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture),
        };
    }
}
