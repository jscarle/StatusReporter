namespace StatusReporter;

internal sealed class GetStatusResponse
{
    public required string Assembly { get; init; }
    public required string VersionId { get; init; }
    public required string Version { get; init; }
    public required string BuiltOn { get; init; }
    public required string StartedOn { get; init; }
    public required string Current { get; init; }
    public required string Uptime { get; init; }
    public required string? TargetFramework { get; init; }
    public required string? Framework { get; init; }
    public required string? Hostname { get; init; }
    public required string? RuntimeIdentifier { get; init; }
    public required string? OperatingSystem { get; init; }
    public required string? Environment { get; init; }
}