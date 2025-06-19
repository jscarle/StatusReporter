using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace StatusReporter;

/// <summary>Provides extension methods for adding and configuring a status reporter in the service collection and endpoints.</summary>
public static class StatusReporterExtensions
{
    /// <summary>Adds the status reporter to the service collection, ensuring it is pre-initialized for accurate uptime reporting.</summary>
    /// <param name="services">The service collection to which the status reporter is added.</param>
    public static void AddStatusReporter(this IServiceCollection services)
    {
        // Pre-initialize the status reporter to accurately report uptime.
        var statusReporter = new StatusReporter();
        _ = statusReporter.GetStatus();
        services.AddSingleton<IStatusReporter, StatusReporter>();
    }

    /// <summary>Maps a status endpoint that returns the current status reported by the <see cref="IStatusReporter"/>.</summary>
    /// <param name="app">The endpoint route builder used to map the status endpoint.</param>
    /// <param name="pattern">The route pattern. Defaults to "status".</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further configure the endpoint.</returns>
    [RequiresUnreferencedCode("This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced.")]
    [RequiresDynamicCode("This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications.")]
    public static RouteHandlerBuilder MapStatus(this IEndpointRouteBuilder app, string pattern = "status")
    {
        return app.MapGet(pattern, GetStatus)
            .Produces<ApplicationStatus>((int)HttpStatusCode.OK, "application/json")
            .WithName(nameof(GetStatus))
            .WithSummary("Gets the current status of the application.")
            .WithTags("Status");
    }

    private static IResult GetStatus([FromServices] IStatusReporter statusReporter)
    {
        var status = statusReporter.GetStatus().ToJson();
        var json = JsonSerializer.Serialize(status, StatusReporterJsonContext.Default.ApplicationStatusJson);
        return Results.Content(json, "application/json", Encoding.UTF8, (int)HttpStatusCode.OK);
    }
}
