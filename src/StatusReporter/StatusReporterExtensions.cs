using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace StatusReporter;

/// <summary>Provides extension methods for adding and configuring a status reporter in the service collection and endpoints.</summary>
public static partial class StatusReporterExtensions
{
    /// <summary>Adds the status reporter to the service collection, ensuring it is pre-initialized for accurate uptime reporting.</summary>
    /// <param name="services">The service collection to which the status reporter is added.</param>
    /// <param name="configureOptions">An optional action to configure the <see cref="StatusReporterOptions"/>.</param>
    public static void AddStatusReporter(this IServiceCollection services, Action<StatusReporterOptions>? configureOptions = null)
    {
        var options = new StatusReporterOptions();
        configureOptions?.Invoke(options);
        ArgumentNullException.ThrowIfNull(options.TimeZone);
        services.AddSingleton(options);

        RuntimeHelpers.RunClassConstructor(typeof(StatusReporter).TypeHandle);
        services.AddSingleton<IStatusReporter, StatusReporter>();
    }

    /// <summary>Maps a status endpoint that returns the current status reported by the <see cref="IStatusReporter"/>.</summary>
    /// <param name="app">The endpoint route builder used to map the status endpoint.</param>
    /// <param name="pattern">The route pattern. Defaults to "status".</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further configure the endpoint.</returns>
    [RequiresUnreferencedCode(
        "This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced."
    )]
    [RequiresDynamicCode(
        "This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications."
    )]
    public static RouteHandlerBuilder MapStatus(this IEndpointRouteBuilder app, string pattern = "status")
    {
        return app.MapGet(pattern, GetStatus)
            .Produces<ApplicationStatus>((int)HttpStatusCode.OK, "application/json")
            .WithName(nameof(GetStatus))
            .WithSummary("Gets the current status of the application.")
            .WithTags("Status");
    }

    private static IResult GetStatus([FromServices] IStatusReporter statusReporter, [FromServices] StatusReporterOptions options)
    {
        var status = statusReporter.GetStatus()
            .ToResponse(options.IncludeSystemInformation);
        var json = JsonSerializer.Serialize(status, StatusReporterJsonContext.Default.GetStatusResponse);
        return Results.Content(json, "application/json", Encoding.UTF8, (int)HttpStatusCode.OK);
    }
    
    private static GetStatusResponse ToResponse(this ApplicationStatus applicationStatus, bool includeSystemInformation)
    {
        return new GetStatusResponse
        {
            Assembly = includeSystemInformation ? applicationStatus.Assembly : null,
            VersionId = applicationStatus.VersionId.ToString("D"),
            Version = applicationStatus.Version,
            BuiltOn = applicationStatus.BuiltOn.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture),
            StartedOn = applicationStatus.StartedOn.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture),
            Current = applicationStatus.Current.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture),
            Uptime = applicationStatus.Uptime.Days > 0
                ? applicationStatus.Uptime.ToString(@"d\.hh\:mm\:ss", CultureInfo.InvariantCulture)
                : applicationStatus.Uptime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture),
            TargetFramework = includeSystemInformation ? applicationStatus.TargetFramework : null,
            Framework = includeSystemInformation ? applicationStatus.Framework : null,
            Hostname = includeSystemInformation ? applicationStatus.Hostname : null,
            RuntimeIdentifier = includeSystemInformation ? applicationStatus.RuntimeIdentifier : null,
            OperatingSystem = includeSystemInformation ? applicationStatus.OperatingSystem : null,
            Environment = includeSystemInformation ? applicationStatus.Environment : null,
        };
    }
    
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(GetStatusResponse))]
    private sealed partial class StatusReporterJsonContext : JsonSerializerContext;
}
